using UnityEngine;
using UnityEngine.Rendering;

public class DriftController : MonoBehaviour
{

    public Material driftMarksMaterial;
    private bool boundsSet, meshUpdated;
    private Color32 black;
    private Color32[] colors;
    private float driftMarkWidth, groundOffset, maxOpacity, minDistance, minSqrDistance;
    private int currentIndex, markIndex, maxDriftMarks;
    private int[] triangles;
    private Mesh marksMesh;
    private MeshFilter meshFilter;
    private MeshRenderer meshRender;
    private Vector2[] uvs;
    private Vector3 distanceDirection, newPosition, xDirection;
    private Vector3[] normals, vertices;
    private Vector4[] tangents;
    class MarkSection
    {
        public Color32 colour;
        public int lastDriftIndex;
        public Vector3 normal = Vector3.zero;
        public Vector3 position = Vector3.zero;
        public Vector3 position1 = Vector3.zero;
        public Vector3 position2 = Vector3.zero;
        public Vector4 tangent = Vector4.zero;
    };
    private MarkSection current, currentSection, last, lastSection;
    private MarkSection[] driftMarks;

    private void Awake()
    {
        black = Color.black;
        groundOffset = 0.02f;
        driftMarkWidth = 0.4f;
        maxDriftMarks = 1000;
        maxOpacity = 0.5f;
        minDistance = 0.75f;
        minSqrDistance = minDistance * minDistance;
    }

    private void Start()
    { CreateDriftMesh(); CreateVectors(); }

    private void LateUpdate()
    {
        if (!meshUpdated) { return; }
        meshUpdated = false;
        UpdateVectors();
        if (!boundsSet)
        {
            marksMesh.bounds = new Bounds(new Vector3(0f, 0f, 0f), new Vector3(10000f, 10000f, 10000f));
            boundsSet = true;
        }
    }

    public int AddDriftMark(Vector3 position, Vector3 normal, float opacity, int lastDriftIndex)
    {
        if (opacity > 1f) { opacity = 1f; }
        else if (opacity < 0f) { return -1; }
        black.a = (byte)(opacity * 255);
        return AddDriftMark(position, normal, black, lastDriftIndex);
    }

    public int AddDriftMark(Vector3 position, Vector3 normal, Color32 colour, int lastDriftIndex)
    {
        if (colour.a == 0) { return -1; }
        lastSection = null;
        distanceDirection = Vector3.zero;
        newPosition = position + normal * groundOffset;
        if (lastDriftIndex != -1)
        {
            lastSection = driftMarks[lastDriftIndex];
            distanceDirection = newPosition - lastSection.position;
            if (distanceDirection.sqrMagnitude < minSqrDistance) { return lastDriftIndex; }
            if (distanceDirection.sqrMagnitude > minSqrDistance * 10f)
            { lastDriftIndex = -1; lastSection = null; }
        }
        colour.a = (byte)(colour.a * maxOpacity);
        currentSection = driftMarks[markIndex];
        currentSection.position = newPosition;
        currentSection.normal = normal;
        currentSection.colour = colour;
        currentSection.lastDriftIndex = lastDriftIndex;
        if (lastSection != null)
        {
            xDirection = Vector3.Cross(distanceDirection, normal).normalized;
            currentSection.position1 = currentSection.position + xDirection * driftMarkWidth * 0.5f;
            currentSection.position2 = currentSection.position - xDirection * driftMarkWidth * 0.5f;
            currentSection.tangent = new Vector4(xDirection.x, xDirection.y, xDirection.z, 1f);
            if (lastSection.lastDriftIndex == -1)
            {
                lastSection.tangent = currentSection.tangent;
                lastSection.position1 = currentSection.position + xDirection * driftMarkWidth * 0.5f;
                lastSection.position2 = currentSection.position - xDirection * driftMarkWidth * 0.5f;
            }
        }
        UpdateDriftMesh();
        currentIndex = markIndex;
        markIndex = ++markIndex % maxDriftMarks;
        return currentIndex;
    }

    private void CreateVectors()
    {
        vertices = new Vector3[maxDriftMarks * 4];
        normals = new Vector3[maxDriftMarks * 4];
        tangents = new Vector4[maxDriftMarks * 4];
        colors = new Color32[maxDriftMarks * 4];
        uvs = new Vector2[maxDriftMarks * 4];
        triangles = new int[maxDriftMarks * 6];
    }

    private void UpdateVectors()
    {
        marksMesh.vertices = vertices;
        marksMesh.normals = normals;
        marksMesh.tangents = tangents;
        marksMesh.triangles = triangles;
        marksMesh.colors32 = colors;
        marksMesh.uv = uvs;
        meshFilter.sharedMesh = marksMesh;
    }

    private void CreateDriftMesh()
    {
        driftMarks = new MarkSection[maxDriftMarks];
        for (int i = 0; i < maxDriftMarks; i++) { driftMarks[i] = new MarkSection(); }
        meshFilter = GetComponent<MeshFilter>();
        meshRender = GetComponent<MeshRenderer>();
        if (meshRender == null) { meshRender = gameObject.AddComponent<MeshRenderer>(); }
        if (meshFilter == null) { meshFilter = gameObject.AddComponent<MeshFilter>(); }
        marksMesh = new Mesh();
        marksMesh.MarkDynamic();
        meshFilter.sharedMesh = marksMesh;
        meshRender.shadowCastingMode = ShadowCastingMode.Off;
        meshRender.receiveShadows = false;
        meshRender.material = driftMarksMaterial;
        meshRender.lightProbeUsage = LightProbeUsage.Off;
        meshRender.reflectionProbeUsage = ReflectionProbeUsage.Off;
    }

    private void UpdateDriftMesh()
    {
        current = driftMarks[markIndex];
        if (current.lastDriftIndex == -1) { return; }
        last = driftMarks[current.lastDriftIndex];
        vertices[markIndex * 4 + 0] = last.position1;
        vertices[markIndex * 4 + 1] = last.position2;
        vertices[markIndex * 4 + 2] = current.position1;
        vertices[markIndex * 4 + 3] = current.position2;
        normals[markIndex * 4 + 0] = last.normal;
        normals[markIndex * 4 + 1] = last.normal;
        normals[markIndex * 4 + 2] = current.normal;
        normals[markIndex * 4 + 3] = current.normal;
        tangents[markIndex * 4 + 0] = last.tangent;
        tangents[markIndex * 4 + 1] = last.tangent;
        tangents[markIndex * 4 + 2] = current.tangent;
        tangents[markIndex * 4 + 3] = current.tangent;
        colors[markIndex * 4 + 0] = last.colour;
        colors[markIndex * 4 + 1] = last.colour;
        colors[markIndex * 4 + 2] = current.colour;
        colors[markIndex * 4 + 3] = current.colour;
        uvs[markIndex * 4 + 0] = new Vector2(0f, 0f);
        uvs[markIndex * 4 + 1] = new Vector2(1f, 0f);
        uvs[markIndex * 4 + 2] = new Vector2(0f, 1f);
        uvs[markIndex * 4 + 3] = new Vector2(1f, 1f);
        triangles[markIndex * 6 + 0] = markIndex * 4 + 0;
        triangles[markIndex * 6 + 2] = markIndex * 4 + 1;
        triangles[markIndex * 6 + 1] = markIndex * 4 + 2;
        triangles[markIndex * 6 + 3] = markIndex * 4 + 2;
        triangles[markIndex * 6 + 5] = markIndex * 4 + 1;
        triangles[markIndex * 6 + 4] = markIndex * 4 + 3;
        meshUpdated = true;
    }

}