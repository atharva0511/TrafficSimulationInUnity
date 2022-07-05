using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class MapManager : MonoBehaviour
{
    bool mapMode = false;
    public Camera radarCamera;
    [Header("Mini Map Details")]
    public float defaultMapSize = 120;
    public float maxMapSize = 1000;
    public float minMapSize = 100;
    public float iconSize = 10;
    public enum MiniMapType { Square,Circular};
    public MiniMapType miniMapType = MiniMapType.Square;

    [Header("Main Map Details")]
    public float cameraMoveSensitivity = 1;

    [Header("Audio Clips")]
    [Tooltip("audio clip that should play on wayoint marking")]
    public AudioClip markWaypointSound;
    [Tooltip("audio clip that should play on icon snapping")]
    public AudioClip snapSound;

    [Header("Components")]
    public AudioSource audioSource;
    public PathFinder pathFinder;
    public MeshFilter pathMeshFilter;
    public GameObject playerWaypoint;

    [Header("UI Components")]
    public GameObject mainMapUI;
    public GameObject miniMapUI;
    public RenderTexture mapTextureLow;
    public RenderTexture mapTextureHigh;

    [Tooltip("LayerMask used to detect radar objects (Radar Layer)")]
    public LayerMask radarLayer;
    [Tooltip("LayerMask used to detect spawn nodes (SpawnTrigger Layer)")]
    public LayerMask spawnTriggerLayer;

    [HideInInspector]
    public RadarIcon hoveredIcon;
    public static List<RadarIcon> RadarIcons;

    Vector2 clickPos;
    Vector3 camPos;
    RadarIcon markedIcon;
    bool waypointMarked = false;
    float moveStiffness = 256;
    int frame = 0;// used for skip frame logic
    bool snapped = false;// used for storing previous snap time
    float snapTime = 0;
    Transform pointingOn;
    public void Awake()
    {
        RadarIcons = new List<RadarIcon>();
    }

    public void SetMapMode(bool mapMode)
    {
        if (!mapMode)
        {
            // on exiting map mode
            radarCamera.orthographicSize = defaultMapSize;
            mainMapUI.SetActive(false);
            miniMapUI.SetActive(true);
            radarCamera.targetTexture = mapTextureLow;
        }
        else
        {
            // on entering map mode
            radarCamera.transform.localEulerAngles = new Vector3(90, 0, 0);
            ResetIconTransforms();
            mainMapUI.SetActive(true);
            miniMapUI.SetActive(false);
            int height = Screen.currentResolution.height;
            mainMapUI.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(height, height);
            radarCamera.targetTexture = mapTextureHigh;
        }
        this.mapMode = mapMode;
    }

    private void Update()
    {

        if (Input.GetButtonDown("Map"))
        {
            PlayerController.ChangePauseStat(!this.mapMode);
            SetMapMode(!this.mapMode);
        }

        if (mapMode)
        {
            // when interacting with map

            // drag start 
            if (Input.GetButtonDown("Fire1"))
            {
                clickPos = Input.mousePosition;
                camPos = radarCamera.transform.position;
            }
            // while dragging
            if (Input.GetButton("Fire1"))
            {
                Vector3 mousePos = Input.mousePosition;
                Vector2 pixelDisplacement = new Vector2(mousePos.x - clickPos.x, mousePos.y - clickPos.y);
                if (false)//Time.unscaledTime > snapTime + 0.3f && snapped)
                {
                    clickPos = Input.mousePosition;
                    camPos = radarCamera.transform.position;
                    pixelDisplacement = new Vector2(mousePos.x - clickPos.x, mousePos.y - clickPos.y);
                    snapped = false;
                }
                else
                {
                    radarCamera.transform.position = new Vector3(camPos.x - (pixelDisplacement.x * 2 * radarCamera.orthographicSize) / moveStiffness,
                      radarCamera.transform.position.y, camPos.z - (pixelDisplacement.y * 2 * radarCamera.orthographicSize) / moveStiffness);
                }
                    
            }
            else
            {
                Vector3 moveVec = cameraMoveSensitivity * radarCamera.orthographicSize * Time.fixedDeltaTime * (new Vector3(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical")));
                if (moveVec.sqrMagnitude > 0)
                {
                    if (true)
                        radarCamera.transform.Translate(moveVec);
                }
            }

            //snap check 
            if (true)
            {
                RaycastHit hit;
                if (Physics.Raycast(radarCamera.transform.position, radarCamera.transform.forward, out hit, 1000, radarLayer))
                {
                    RadarIcon icon = hit.transform.GetComponent<RadarIcon>();
                    if (icon != null)
                    {
                        hoveredIcon = icon;
                        if (pointingOn != hit.transform)
                        {
                            //radarCamera.transform.position = new Vector3(icon.transform.position.x, radarCamera.transform.position.y, icon.transform.position.z);
                            if (snapSound != null)
                            {
                                audioSource.clip = snapSound;
                                audioSource.pitch = 1f;
                                audioSource.Play();
                                snapTime = Time.unscaledTime;
                                snapped = true;
                            }
                        }
                    }
                    else
                        hoveredIcon = null;
                    pointingOn = hit.transform;
                }
                else
                {
                    hoveredIcon = null;
                    pointingOn = null;
                }
            }

            // Zoom in and out on mouse wheel scroll
            radarCamera.orthographicSize = Mathf.Clamp(radarCamera.orthographicSize - Input.GetAxis("Mouse ScrollWheel") * 120,minMapSize,maxMapSize);

            //mark and remove waypoint
            if (Input.GetButtonDown("Submit"))
            {
                if (waypointMarked)
                {
                    // remove waypoint
                    markedIcon = null;
                    pathMeshFilter.mesh.Clear();
                    playerWaypoint.SetActive(false);
                    pathFinder.endNode = null;
                    waypointMarked = false;
                    audioSource.clip = markWaypointSound;
                    audioSource.pitch = 1f;
                    audioSource.Play();
                }
                else
                {
                    // mark waypoint
                    waypointMarked = true;
                    playerWaypoint.SetActive(true);
                    RaycastHit hit;
                    if (Physics.Raycast(radarCamera.transform.position, radarCamera.transform.forward,out hit, 1000, radarLayer))
                    {
                        RadarIcon icon = hit.transform.GetComponent<RadarIcon>();
                        if (icon != null && icon.markable)
                        {
                            markedIcon = icon;
                            playerWaypoint.transform.position = icon.transform.position-0.5f*Vector3.up;
                        }
                        else
                        {
                            markedIcon = null;
                            playerWaypoint.transform.position = hit.point + Vector3.up;
                        }
                    }
                    else
                    {
                        markedIcon = null;
                        Vector3 pos = radarCamera.transform.position;
                        pos.y = 0;
                        playerWaypoint.transform.position = pos;
                    }
                    audioSource.clip = markWaypointSound;
                    audioSource.pitch = 1f;
                    audioSource.Play();
                    // find end node
                    StartCoroutine(FindEndNode(playerWaypoint.transform.position));
                }
                Physics.SyncTransforms();
            }

            //exit map mode
            if (Input.GetButtonDown("Cancel"))
            {
                SetMapMode(false);PlayerController.ChangePauseStat(false);
            }
        }

        else
        {
            if (markedIcon != null)
            {
                if (markedIcon.dynamicIcon)
                    playerWaypoint.transform.position = markedIcon.transform.position - 0.5f * Vector3.up;
            }
            //float startTime = Time.realtimeSinceStartup;
            //set icon transforms
            if (miniMapType == MiniMapType.Square)
            {
                foreach (RadarIcon radarIcon in RadarIcons)
                {
                    Transform icon = radarIcon.transform.GetChild(0);
                    if (!radarIcon.dynamicIcon)
                    {
                        icon.eulerAngles = new Vector3(icon.eulerAngles.x, radarCamera.transform.eulerAngles.y+180, icon.eulerAngles.z);
                    }
                    if (radarIcon.boundInMiniMap || radarIcon == markedIcon)
                    {
                        //snap icon in minimap
                        Vector3 relativePos = radarCamera.transform.InverseTransformPoint(radarIcon.transform.position);
                        icon.transform.position = radarCamera.transform.TransformPoint(EvaluateSnapPositionSquare(relativePos, defaultMapSize));
                    }
                }
            }
            else if(miniMapType == MiniMapType.Circular)
            {
                foreach (RadarIcon radarIcon in RadarIcons)
                {
                    if (radarIcon.boundInMiniMap || radarIcon == markedIcon)
                    {
                        //snap icon in minimap
                        Transform icon = radarIcon.transform.GetChild(0);
                        Vector3 relativePos = radarCamera.transform.InverseTransformPoint(radarIcon.transform.position);
                        icon.transform.position = radarCamera.transform.TransformPoint(EvaluateSnapPositionCircle(relativePos, defaultMapSize));
                    }
                }
            }
            //Debug.Log(((Time.realtimeSinceStartup - startTime) * 1000) + "ms");
        }
    }

    private void FixedUpdate()
    {
        if (frame >= 30)
        {
            if (markedIcon != null)
            {// when marker keeps moving--> update end node
                if (markedIcon.dynamicIcon)
                {
                    Vector3 fromPos = markedIcon.transform.position;
                    RaycastHit hit;
                    if (Physics.SphereCast(fromPos + 100 * Vector3.up, 20, -Vector3.up, out hit, 500, spawnTriggerLayer))
                    {
                        if (hit.transform.gameObject.tag == "Node")
                        {
                            pathFinder.endNode = hit.transform.GetComponent<Node>();
                        }
                    }
                }
            }
            frame = 0;
        }
        frame += 1;
    }

    IEnumerator FindEndNode(Vector3 fromPos)
    {
        yield return new WaitForSecondsRealtime(0.25f);
        Collider[] cols = Physics.OverlapSphere(fromPos, 100, spawnTriggerLayer);
        float minDistSqr = float.MaxValue;
        Collider nearestCol = null;
        foreach (Collider col in cols)
        {
            if(col.gameObject.CompareTag("Node"))
            {
                float distSqr = (col.transform.position - fromPos).sqrMagnitude;
                if (distSqr < minDistSqr)
                {
                    minDistSqr = distSqr;
                    nearestCol = col;
                }
            }
        }
        if(nearestCol!=null)
            pathFinder.endNode = nearestCol.GetComponent<Node>();
        pathFinder.currentPath = pathFinder.FindPath(pathFinder.startNode,pathFinder.endNode);
        pathFinder.DrawPathMesh(pathFinder.currentPath, pathMeshFilter, 6);
    }

    Vector3 EvaluateSnapPositionSquare(Vector3 relativePos,float semiSize)
    {
        float x = relativePos.x;
        float y = relativePos.y;
        bool isWithin =  (math.abs(x) < semiSize - iconSize * 0.5f) && (math.abs(y) < semiSize - iconSize * 0.5f);
        Vector3 snapPos = new Vector3(x, y, relativePos.z);
        if (!isWithin)
        {
            float tan = y / x;
            if (math.abs(tan) <1)
            {
                snapPos.x = (x > 0 ? 1 : -1) * (semiSize - iconSize * 0.5f);
                snapPos.y = snapPos.x * tan;
            }
            else
            {
                snapPos.y = (y > 0 ? 1 : -1) * (semiSize - iconSize * 0.5f);
                snapPos.x = snapPos.y / tan;
            }
        }
        return snapPos;// in camera frame reference
    }

    Vector3 EvaluateSnapPositionCircle(Vector3 relativePos, float semiSize)
    {
        float x = relativePos.x;
        float y = relativePos.y;
        float rMax = (semiSize - iconSize * 0.5f);
        bool isWithin = x * x + y * y < rMax*rMax;
        Vector3 snapPos = new Vector3(x, y, relativePos.z);
        if (!isWithin)
        {
            float angle = -Vector2.SignedAngle(new Vector2(x,y), new Vector2(1, 0));
            float tan = y / x;
            snapPos.x = rMax * math.cos(math.radians(angle));
            snapPos.y = rMax * math.sin(math.radians(angle));
        }
        return snapPos;// in camera frame reference
    }

    void ResetIconTransforms()
    {
        foreach (RadarIcon radarIcon in RadarIcons)
        {
            if (radarIcon.transform.childCount == 0) continue;
            Transform icon = radarIcon.transform.GetChild(0);
            icon.localPosition = Vector3.zero;
            icon.localEulerAngles = new Vector3(-90,0,180);
        }
        Physics.SyncTransforms();
    }
}
