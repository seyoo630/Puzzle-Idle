using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.EventSystems;
using System.Linq;

public class InputController : MonoBehaviour
{
    private Block block;
    private Camera mainCamera;
    private Vector3 startPosition;
    private SpriteRenderer sr;
    private int originalOrder;
    private Tile startTile;
    private bool isDragging = false;


    [SerializeField] private float dragRadius = 0.8f;

    private void Awake()
    {
        block = GetComponent<Block>();
        mainCamera = Camera.main;
        sr = GetComponent<SpriteRenderer>();
    }

    private void OnMouseDrag()
    {
        if (!isDragging) return;
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        Vector3 offset = mousePos - startPosition;
        if (offset.magnitude > dragRadius)
            offset = offset.normalized * dragRadius;

        transform.position = startPosition + offset;
    }

    private void OnMouseUp()
    {
        if (!isDragging || startTile == null)
        {
            ResetPosition();
            return;
        }

        isDragging = false;

        var neighbors = GameManager.Instance.board.GetAdjacentTiles(startTile);
        if (neighbors == null || neighbors.Count == 0)
        {
            ResetPosition();
            return;
        }

        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        Tile closestNeighbor = neighbors
            .OrderBy(n => Vector2.Distance(mousePos, n.transform.position))
            .FirstOrDefault();

        if (closestNeighbor == null)
        {
            ResetPosition();
            return;
        }

        float dist = Vector2.Distance(mousePos, closestNeighbor.transform.position);
        if (dist > 1f)
        {
            ResetPosition();
            return;
        }

        StartCoroutine(TrySwapAndCheck(startTile, closestNeighbor));
    }

    private IEnumerator TrySwapAndCheck(Tile a, Tile b)
    {
        SwapBlocks(a, b);
        yield return new WaitForSeconds(0.1f);

        //GameManager.Instance.CheckMatch();

        //if (!GameManager.Instance.HasRecentMatch)
        //{
        //    SwapBlocks(a, b); 
        //}
        if (sr != null)
            sr.sortingOrder = originalOrder;
    }

    private void SwapBlocks(Tile a, Tile b)
    {
        Block blockA = a.currentBlock;
        Block blockB = b.currentBlock;

        if (blockA == null || blockB == null)
        {
            ResetPosition();
            return;
        }

        a.currentBlock = blockB;
        b.currentBlock = blockA;

        blockA.transform.position = b.transform.position;
        blockB.transform.position = a.transform.position;
    }

    private void ResetPosition()
    {
        transform.position = startPosition;
    }
}
