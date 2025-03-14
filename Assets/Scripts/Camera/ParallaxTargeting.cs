using UnityEngine;

public class ParallaxTargeting : MonoBehaviour
{
  public const float LARGE_NUMBER = 10000f;

  [SerializeField]
  float perceivedDistance = 10f;

  [SerializeField]
  bool onlyParallaxOnX = false;

  [SerializeField]
  float minXDelta = -LARGE_NUMBER;

  [SerializeField]
  float maxXDelta = LARGE_NUMBER;

  [SerializeField]
  float minYDelta = -LARGE_NUMBER;

  [SerializeField]
  float maxYDelta = LARGE_NUMBER;

  public float minX;
  public float maxX;
  public float minY;
  public float maxY;

  private Vector3 startingPosition;
  private Transform mainCamera;
  private Transform thisTransform;

  void Awake()
  {
    mainCamera = Camera.main.transform;
    thisTransform = transform;

    startingPosition = thisTransform.position;
    minX = minXDelta + startingPosition.x;
    maxX = maxXDelta + startingPosition.x;
    minY = minYDelta + startingPosition.y;
    maxY = maxYDelta + startingPosition.y;
  }

  void Update()
  {
    var inverseOfDistance = 1f / perceivedDistance;
    var deltaChange =
      (mainCamera.position - startingPosition) * inverseOfDistance;
    var newX = mainCamera.position.x - deltaChange.x;
    var newY = onlyParallaxOnX
      ? thisTransform.position.y
      : mainCamera.position.y - deltaChange.y;
    var newPosition = new Vector3(
      Mathf.Min(Mathf.Max(newX, minX), maxX),
      Mathf.Min(Mathf.Max(newY, minY), maxY),
      thisTransform.position.z
    );
    thisTransform.position = Vector2.Lerp(
      thisTransform.position,
      newPosition,
      Time.deltaTime * 100
    );
  }
}
