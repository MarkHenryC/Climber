using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.UIElements;
using ICSharpCode.NRefactory.Ast;

namespace QuiteSensible
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshCollider))]
    public class LevelCreator : MonoBehaviour
    {
        public int xSize = 60, zSize = 60;
        public bool autoCreate = true;
        public int landscapeHeight = 5;
        public Color defaultColor = Color.blue;
        public Color highlightColor = Color.red;
        public Gradient gradient;
        public bool useGradient;
        public GameObject landingSquare;
        public LayerMask ourLayer;
        public Vector3 boxCastExtents;
        [Tooltip("How the tiles spread out from the centre.")]
        public AnimationCurve distanceCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f)); // default linear       

        private readonly Dictionary<int, PositionData> positionGrid = new Dictionary<int, PositionData>();
        private Vector3[] vertices;
        private Mesh mesh;
        private LandingQuad[] quads;
        private int xPanels, zPanels;
        private int xStride;
        private float timer;
        private int triangleHitIndex = -1;
        private Color32[] colorArray;
        private Color32[] colorBuffer;
        private int highestPointPanelIndex = 0, lowestPointPanelIndex = System.Int32.MaxValue;
        private bool ready;
        private bool destHeadPosShowing;
        private Vector3 destHeadPos;

        private void Start()
        {
            if (!landingSquare)
            {
                landingSquare = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            }
#if UNITY_EDITOR
            if (autoCreate)
                CreateLandscapeMesh();
#endif
        }

        public int HighestPanel => highestPointPanelIndex;
        public int LowestPanel => lowestPointPanelIndex;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="direction"></param>
        /// <param name="distance"></param>
        /// <param name="layer"></param>
        /// <param name="lookForObstruction">If we're about to move to a valid destination, check if there are any obstructions</param>
        public PositionData Scan(Vector3 startPoint, Vector3 direction, float distance, float spherecastRadius, bool lookForObstruction = false)
        {
            if (!ready)
                return null;

            //bool gotHit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, distance, raycastLayer);

            bool gotHit = Physics.Raycast(startPoint, direction, out RaycastHit hit, distance, ourLayer);

            if (gotHit)
            {
                int hitObstructionTriangleIndex = -1;
                Vector3 hitObstructionLocation = Vector3.zero;
                bool possibleObstruction = false;

                int tIndex = LandingQuad.StartTri(hit.triangleIndex);
                PositionData pd = FindPositionData(tIndex);
                if (pd != null)
                {
                    triangleHitIndex = tIndex;

                    if (lookForObstruction)
                    {
                        Vector3 destHeadPos = pd.globalCentrePos;
                        destHeadPos.y += 2f; // test
                        Vector3 obstructionDirection = destHeadPos - startPoint;
                        possibleObstruction = Physics.SphereCast(startPoint, spherecastRadius, obstructionDirection,
                            out RaycastHit hitObstruction, distance);
                        //possibleObstruction = Physics.BoxCast(startPoint, boxCastExtents, direction,
                        //    out RaycastHit hitObstruction, Quaternion.identity, distance);

                        if (hitObstruction.distance >= hit.distance)
                            possibleObstruction = false;
                        if (possibleObstruction)
                        {
                            hitObstructionTriangleIndex = hitObstruction.triangleIndex;
                            hitObstructionLocation = hitObstruction.point;
                            
                            Debug.DrawLine(startPoint, destHeadPos, Color.white, 2f);
                            
                        }
                    }

                    if (possibleObstruction)
                    {
                        destHeadPosShowing = true;

                        int oIndex = LandingQuad.StartTri(hitObstructionTriangleIndex);
                        if (oIndex != tIndex && oIndex >= 0)
                        {
                            Debug.LogFormat("Dest index: {0}, obstruction index: {1}", tIndex, oIndex);

                            pd.obstructed = true;
                            pd.obstructionLocation = hitObstructionLocation;
                            HighlightTri(oIndex);
                        }
                    }
                    else
                        destHeadPosShowing = false;
                    SelectQuad(hit.collider.transform, pd);
                    return pd;
                }
                else
                {
                    if (triangleHitIndex >= 0)
                        DeselectQuad();

                    triangleHitIndex = -1;
                }
            }
            else
            {
                if (triangleHitIndex >= 0)
                {
                    Debug.Log("Clearing previous selection");

                    // Notify no longer pointing at valid space
                    DeselectQuad();
                    triangleHitIndex = -1;
                }
            }
            return null;
        }

        private void SelectQuad(Transform hitTransform, PositionData pd)
        {
            landingSquare.SetActive(true);

            pd.globalCentrePos = hitTransform.TransformPoint(pd.centrePos);

            landingSquare.transform.position = pd.globalCentrePos;

#if HIGHLIGHT_LANDING_VERTICES
            colorBuffer[pd.landingQuad.ixBottomLeft] = highlightColor;
            colorBuffer[pd.landingQuad.ixTopLeft] = highlightColor;
            colorBuffer[pd.landingQuad.ixTopRight] = highlightColor;
            colorBuffer[pd.landingQuad.ixBottomRight] = highlightColor;

            mesh.colors32 = colorBuffer;
#endif

        }

        private void DeselectQuad()
        {
            landingSquare.SetActive(false);
            Array.Copy(colorArray, colorBuffer, vertices.Length);

            mesh.colors32 = colorBuffer;
        }

        private void HighlightTri(int triNumber)
        {
            int firstTriVertex = triNumber * 3;

            colorBuffer[mesh.triangles[firstTriVertex]] = highlightColor;
            colorBuffer[mesh.triangles[firstTriVertex] + 1] = highlightColor;
            colorBuffer[mesh.triangles[firstTriVertex] + 2] = highlightColor;

            mesh.colors32 = colorBuffer;
        }

        public PositionData FindPositionData(int triangleIndex)
        {
            if (positionGrid.ContainsKey(triangleIndex))
                return positionGrid[triangleIndex];
            else
                return null;
        }

        // Create the mesh and position data. Does not
        // rely on Platform prefab. This is what is now
        // being used in the reworked code. Energy will 
        // be stored in active actors rather than the
        // platforms. Actors will be tracked by a table
        // storing all viable positions (level quads)
        public void CreateLandscapeMesh()
        {
            timer = Time.realtimeSinceStartup;
            Debug.Log("Starting timer in CreateLandscapeMesh");

            GetComponent<MeshFilter>().mesh = mesh = new Mesh();
            mesh.name = "Landscape";

            vertices = new Vector3[(xSize + 1) * (zSize + 1)];
            Vector2[] uv = new Vector2[vertices.Length];
            Vector4[] tangents = new Vector4[vertices.Length];
            Vector4 tangent = new Vector4(1f, 0f, 0f, -1f);

            colorArray = new Color32[vertices.Length];
            colorBuffer = new Color32[vertices.Length];

            // Landing positions are on each alternate quad in x & z
            xPanels = (int)(xSize / 2.0f);
            zPanels = (int)(zSize / 2.0f);
            quads = new LandingQuad[xPanels * zPanels];

            int vertexIndex = 0;
            int panelIndex = 0;
            xStride = xSize + 1;

            float midZ = (zSize + 1f) / 2f;
            float midX = (xSize + 1f) / 2f;
            float maxDistance = Vector2.Distance(Vector2.zero, new Vector2(xSize + 1, zSize + 1));

            for (int z = 0; z <= zSize; z++)
            {
                float curZ = z - midZ;

                for (int x = 0; x <= xSize; x++)
                {
                    float curX = x - midX;
                    float distanceFromCentreCoeff = Vector2.Distance(Vector2.zero, new Vector2(curX, curZ)) / maxDistance;
                    float coeff = distanceCurve.Evaluate(distanceFromCentreCoeff);

                    float scaledX = curX * coeff;
                    float scaledZ = curZ * coeff;

                    vertices[vertexIndex] = new Vector3(scaledX, 0, scaledZ);
                    uv[vertexIndex] = new Vector2((float)x / xSize, (float)z / zSize);
                    tangents[vertexIndex] = tangent;

                    if (x > 0 && z > 0 && x % 2 == 0 && z % 2 == 0)
                    {
                        Debug.Assert(vertexIndex == xStride * z + x);

                        int firstVertexIndex = xStride * (z - 1) + x - 1;

                        var lq = new LandingQuad
                        {
                            ixBottomLeft = firstVertexIndex,
                            ixTopLeft = firstVertexIndex + xStride,
                            ixTopRight = vertexIndex,
                            ixBottomRight = firstVertexIndex + 1,
                            // Need to work out the triangle index that corresponds
                            // with the index returned from a raycast. Raycast index
                            // returned is virtual triangle. This needs to be 
                            // converted to an index that can query the tri array
                            // by multiplying the index by 3
                            startTriangleIndex = (z - 1) * (xStride - 1) * 2 + x * 2 - 2
                        };

                        quads[panelIndex] = lq;

                        panelIndex++;
                    }
                    vertexIndex++;
                }
            }

            Debug.Log("Elapsed after panel creation: " + (Time.realtimeSinceStartup - timer));
            timer = Time.realtimeSinceStartup;

            Debug.Log("Elapsed after adding vertices: " + (Time.realtimeSinceStartup - timer));
            timer = Time.realtimeSinceStartup;

            // Make quads out of tri pairs starting bottom left
            int[] triangles = new int[xSize * zSize * 6];
            for (int ti = 0, vi = 0, z = 0; z < zSize; z++, vi++)
            {
                for (int x = 0; x < xSize; x++, ti += 6, vi++)
                {
                    triangles[ti] = vi;
                    triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                    triangles[ti + 4] = triangles[ti + 1] = vi + xSize + 1;
                    triangles[ti + 5] = vi + xSize + 2;
                }
            }

            Debug.Log("Elapsed after adding triangles: " + (Time.realtimeSinceStartup - timer));
            timer = Time.realtimeSinceStartup;

            float highestPoint = 0.0f, lowestPoint = landscapeHeight;
            int i = 0;
            float yRange = landscapeHeight * .25f;

            for (int z = 0; z < zPanels; z++)
            {
                for (int x = 0; x < xPanels; x++)
                {
                    float zEdgeProximity = 1f - NormalizedDistanceFromMid(z, zPanels - 1f);
                    float xEdgeProximity = 1f - NormalizedDistanceFromMid(x, xPanels - 1f);

                    float closest = Mathf.Min(zEdgeProximity, xEdgeProximity);
                    float topRange = landscapeHeight * closest; //approaches 1 toward middle
                    float botRange = topRange - yRange; // the range of randomness
                    if (z == 0 || x == 0)
                    {
                        botRange = 0f;
                        topRange = botRange + landscapeHeight * 0.01f;
                    }
                    var y = Mathf.Max(0f, UnityEngine.Random.Range(botRange, topRange));

                    var pIndex = z * xPanels + x;

                    var quad = quads[pIndex];
                    vertices[quad.ixBottomLeft].y = y;
                    vertices[quad.ixTopLeft].y = y;
                    vertices[quad.ixTopRight].y = y;
                    vertices[quad.ixBottomRight].y = y;

                    Color col = defaultColor;
                    if (useGradient)
                        col = gradient.Evaluate(closest);

                    colorArray[quad.ixBottomLeft] = col;
                    colorArray[quad.ixTopLeft] = col;
                    colorArray[quad.ixTopRight] = col;
                    colorArray[quad.ixBottomRight] = col;

                    positionGrid[quad.startTriangleIndex] = new PositionData
                    {
                        centrePos = Vector3.Lerp(vertices[quad.ixTopLeft], vertices[quad.ixBottomRight], .5f),
                        landingQuad = quad
                    };

                    if (y > highestPoint)
                    {
                        highestPoint = y;
                        highestPointPanelIndex = i;
                    }
                    if (y < lowestPoint)
                    {
                        lowestPoint = y;
                        lowestPointPanelIndex = i;
                    }
                }

            }

            Debug.Log("Elapsed after adding Panels: " + (Time.realtimeSinceStartup - timer));
            timer = Time.realtimeSinceStartup;

            Array.Copy(colorArray, colorBuffer, vertices.Length);

            mesh.vertices = vertices;
            //mesh.uv = uv;
            mesh.tangents = tangents;
            mesh.triangles = triangles;
            mesh.colors32 = colorBuffer;

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            Debug.Log("Elapsed after adding landscape mesh: " + (Time.realtimeSinceStartup - timer));
            timer = Time.realtimeSinceStartup;

            Debug.Log("Highest, lowest: " + highestPoint + ", " + lowestPoint);

            Debug.Log("Elapsed after adding energy objects: " + (Time.realtimeSinceStartup - timer));
            timer = Time.realtimeSinceStartup;

            var renderer = GetComponent<MeshRenderer>();
            if (renderer)
                renderer.enabled = true;
            var collider = GetComponent<MeshCollider>();
            if (collider)
                collider.sharedMesh = mesh;

            ready = true;
        }

        public static float NormalizedDistanceFromMid(float val, float max)
        {
            return Mathf.Abs(ConvertMinusOneToPlusOneRange(val, max));
        }

        public static float ConvertMinusOneToPlusOneRange(float val, float max)
        {
            return ConvertZeroToOneRange(val, max) * 2f - 1f;
        }

        public static float ConvertZeroToOneRange(float val, float max)
        {
            return val / max;
        }
    }
}