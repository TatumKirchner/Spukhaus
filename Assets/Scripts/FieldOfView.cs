using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    #region Parameters
    public float viewRadius;
    [Range(0, 360)]
    public float viewAngle;

    public LayerMask targetMask;
    public LayerMask obstacleMask;

    public List<Transform> visibleTargets = new List<Transform>();

    public float meshResolution;
    public int edgeResolveIterations;
    public float edgeDistanceThreshold;

    public MeshFilter viewMeshFilter;
    Mesh viewMesh;
    public Flashlight flashlight;

    public float fadeTime = .2f;
    private Material targetMaterial;
    private Color targetColor;

    private bool setAlpha = false;

    private GameObject currentFadeObject;
    private GameObject currentAnimatedObject;
    private Pause pause;
    private bool endGame = false;
    #endregion

    private void Start()
    {
        viewMesh = new Mesh();
        viewMesh.name = "ViewMesh";
        viewMeshFilter.mesh = viewMesh;
        pause = FindObjectOfType<Pause>();
    }

    private void OnEnable()
    {
        StartCoroutine(FindTargets(0.2f));
    }

    private void LateUpdate()
    {
        DrawFieldOfView();
    }

    private void Update()
    {
        StepAlpha();
    }

    //Runs a while loop to detect intersection with the view mesh. Used a coroutine to limit how many times it runs to help with performance
    IEnumerator FindTargets(float delay)
    {        
        while (true)
        {
            yield return new WaitForSeconds(delay);
            FindVisibleTargets();
        }        
    }

    void FindVisibleTargets()
    {
        //Clear the list and repopulate with the new colliders in the overlap sphere
        visibleTargets.Clear();
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            //Get the direction to the target
            Transform target = targetsInViewRadius[i].transform;
            Vector3 directionToTarget = (target.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, directionToTarget) < viewAngle / 2)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);
                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleMask))
                {
                    //do something when target is visible
                    visibleTargets.Add(target);
                    if (target.CompareTag("Fade Object"))
                    {
                        currentFadeObject = target.gameObject;
                        targetMaterial = target.GetComponent<MeshRenderer>().material;
                        targetColor = targetMaterial.color;
                        targetColor.a = 0;
                        setAlpha = true;
                    }

                    if (target.CompareTag("Animated Object"))
                    {
                        currentAnimatedObject = target.parent.gameObject;
                        StartCoroutine(DisableObject(2f));
                    }

                    if (target.CompareTag("Play Anim"))
                    {
                        if (!endGame)
                        {
                            endGame = true;
                            currentAnimatedObject = target.parent.gameObject;
                            Animator anim = currentAnimatedObject.GetComponent<Animator>();
                            AudioSource source = currentAnimatedObject.GetComponent<AudioSource>();
                            anim.SetBool("playAnim", true);
                            source.PlayOneShot(source.clip);
                            StartCoroutine(DisableObject(2f));
                        }
                    }
                }
            }
        }
    }

    IEnumerator DisableObject(float time)
    {
        yield return new WaitForSeconds(time);
        currentAnimatedObject.SetActive(false);
        if (endGame)
            pause.EndScreen();
    }

    //If the target is a fade object lerp its alpha to make it transparent
    void StepAlpha()
    {
        if (setAlpha && targetMaterial.color.a >= 0)
        {
            targetMaterial.color = Color.Lerp(targetMaterial.color, targetColor, fadeTime * Time.deltaTime);
            if (targetMaterial.color.a <= 0.01f)
            {
                currentFadeObject.SetActive(false);
                setAlpha = false;
            }
        }
        
    }

    //Draws a mesh in front of the player to detect objects in the field of view
    void DrawFieldOfView()
    {
        int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
        float stepAngleSize = viewAngle / stepCount;
        List<Vector3> viewPoints = new List<Vector3>();
        ViewCastInfo oldViewCast = new ViewCastInfo();

        for (int i = 0; i <= stepCount; i++)
        {
            float angle = transform.eulerAngles.y - viewAngle / 2 + stepAngleSize * i;
            ViewCastInfo newViewCast = ViewCast(angle);

            if (i > 0)
            {
                bool edgeDistanceThresholdExceeded = Mathf.Abs(oldViewCast.distance - newViewCast.distance) > edgeDistanceThreshold;
                if (oldViewCast.hit != newViewCast.hit || (oldViewCast.hit && newViewCast.hit && edgeDistanceThresholdExceeded))
                {
                    EdgeInfo edge = FindEdge(oldViewCast, newViewCast);
                    if (edge.pointA != Vector3.zero)
                        viewPoints.Add(edge.pointA);
                    if (edge.pointB != Vector3.zero)
                        viewPoints.Add(edge.pointB);
                }
            }

            viewPoints.Add(newViewCast.point);
            oldViewCast = newViewCast;
        }

        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero;
        for (int i = 0; i < vertexCount - 1; i++)
        {
            vertices[i + 1] = -transform.InverseTransformPoint(viewPoints[i]);

            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        viewMesh.Clear();
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
    }

    //When the mesh hits an edge this will recalculate the mesh to draw it smoother on the edge
    EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast)
    {
        float minAngle = minViewCast.angle;
        float maxAngle = maxViewCast.angle;
        Vector3 minPoint = Vector3.zero;
        Vector3 maxPoint = Vector3.zero;

        for (int i = 0; i < edgeResolveIterations; i++)
        {
            float angle = (minAngle + maxAngle) / 2;
            ViewCastInfo newViewCast = ViewCast(angle);

            bool edgeDistanceThresholdExceeded = Mathf.Abs(minViewCast.distance - newViewCast.distance) > edgeDistanceThreshold;
            if (newViewCast.hit == minViewCast.hit && !edgeDistanceThresholdExceeded)
            {
                minAngle = angle;
                minPoint = newViewCast.point;
            }
            else
            {
                maxAngle = angle;
                maxPoint = newViewCast.point;
            }
        }

        return new EdgeInfo(minPoint, maxPoint);
    }

    //Returns info from a raycast along the field of view mesh
    ViewCastInfo ViewCast(float globalAngle)
    {
        Vector3 direction = DirectionFromAngle(globalAngle, true);
        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, viewRadius, obstacleMask))
        {
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
        }
        else
        {
            return new ViewCastInfo(false, transform.position + direction * viewRadius, viewRadius, globalAngle);
        }
    }

    public struct ViewCastInfo
    {
        public bool hit;
        public Vector3 point;
        public float distance;
        public float angle;

        public ViewCastInfo(bool _hit, Vector3 _point, float _distance, float _angle)
        {
            hit = _hit;
            point = _point;
            distance = _distance;
            angle = _angle;
        }
    }

    public Vector3 DirectionFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
            angleInDegrees += transform.eulerAngles.y;

        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    public struct EdgeInfo
    {
        public Vector3 pointA;
        public Vector3 pointB;

        public EdgeInfo(Vector3 _pointA, Vector3 _pointB)
        {
            pointA = _pointA;
            pointB = _pointB;
        }
    }
}
