using System.Collections.Generic;
using UnityEngine;
using System;

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
        public Color highlightColor = Color.white;
        public Gradient gradient;
        public bool useGradient;
        public LayerMask ourLayer;
        public GameData gameData;
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
        private int[] positionIndices;

        private int highestPointPanelIndex, lowestPointPanelIndex;
        private bool ready;

        private void Start()
        {
            highestPointPanelIndex = 0;
            lowestPointPanelIndex = System.Int32.MaxValue;

#if UNITY_EDITOR
            if (autoCreate)
                CreateLandscapeMesh();
#endif
        }

        public int HighestPanel => LandingQuad.StartTri(highestPointPanelIndex);
        public int LowestPanel => LandingQuad.StartTri(lowestPointPanelIndex);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="direction"></param>
        /// <param name="distance"></param>
        /// <param name="layer"></param>
        /// <param name="lookForObstruction">If we're about to move to a valid destination, check if there are any obstructions</param>
        public PositionData Scan(Vector3 startPoint, Vector3 direction, float distance)
        {
            if (!ready)
                return null;

            int newHitIndex = -1;
            PositionData newPosition = null;

            bool gotHit = Physics.Raycast(startPoint, direction, out RaycastHit hit, distance, ourLayer);

            if (gotHit) // New hit
            {
                newPosition = FindPositionData(hit.triangleIndex);
                if (newPosition != null)
                {
                    // It's a valid landing spot.
                    newHitIndex = newPosition.landingQuad.startTriangleIndex;
                }
            }

            if (newHitIndex != triangleHitIndex)
            {
                ClearCurrentTarget();

                if (newHitIndex >= 0)
                    HighlightQuad(newPosition);

                triangleHitIndex = newHitIndex;
                UpdateColors();
            }

            return newPosition;
        }

        private void HighlightQuad(PositionData pd)
        {           
            colorBuffer[pd.landingQuad.ixBottomLeft] = highlightColor;
            colorBuffer[pd.landingQuad.ixTopLeft] = highlightColor;
            colorBuffer[pd.landingQuad.ixTopRight] = highlightColor;
            colorBuffer[pd.landingQuad.ixBottomRight] = highlightColor;            
        }

        private void UpdateColors()
        {
            mesh.colors32 = colorBuffer;
        }

        private void ResetColorBuffer()
        {
            Array.Copy(colorArray, colorBuffer, vertices.Length);
        }

        public PositionData GetCurrentTarget()
        {
            if (triangleHitIndex >= 0)
                return FindPositionData(triangleHitIndex);

            return null;
        }

        public void PlaceOccupant(PositionData pd, PositionData.OccupantType ot)
        {
            Debug.LogFormat("Placing occupant {0} at index {1}", ot, pd.landingQuad.startTriangleIndex);

            pd.occupant = ot;
        }

        public PositionData SetPlayerAt()
        {
            if (triangleHitIndex >= 0)
            {
                PositionData current = FindPositionData(triangleHitIndex);
                if (current != null)
                {
                    PlaceOccupant(current, PositionData.OccupantType.Player);
                    return current;
                }
            }
            return null;
        }

        public void ClearCurrentTarget()
        {
            if (triangleHitIndex >= 0)
            {
                ResetColorBuffer();

                PositionData prev = FindPositionData(triangleHitIndex);
                if (prev != null)
                {
                    if (prev.occupant == PositionData.OccupantType.Player)
                    {
                        Debug.LogFormat("Clearing Player occupancyat index {0}", prev.landingQuad.startTriangleIndex);

                        prev.occupant = PositionData.OccupantType.None;
                    }
                }
            }
        }

        public PositionData FindPositionData(int triangleIndex)
        {
            int tIndex = LandingQuad.StartTri(triangleIndex);
            if (positionGrid.ContainsKey(tIndex))
            {
                Debug.Assert(positionGrid[tIndex].landingQuad.startTriangleIndex == tIndex);

                return positionGrid[tIndex];
            }
            else
                return null;
        }

        public PositionData SetObjectAt(Transform thing, PositionData.OccupantType ot, int index)
        {
            PositionData pd = FindPositionData(index);
            if (pd != null)
            {
                Debug.LogFormat("Placing occupant {0} at index {1}", ot, index);

                thing.transform.position = transform.TransformPoint(pd.centrePos);
                pd.occupant = ot;
                return pd;
            }
            return null;
        }

        public bool CreateObjectAt(GameObject template, PositionData.OccupantType ot, int index)
        {
            PositionData pd = FindPositionData(index);
            if (pd != null)
            {
                Debug.LogFormat("Creating occupant {0} at index {1}", ot, index);

                if (pd.occupant != PositionData.OccupantType.None)
                {
                    Debug.LogWarningFormat("Trying to place {0} but position occupied by {1}!", ot, pd.occupant);
                    return false;
                }
                GameObject go = gameData.GetObject(template);
                go.transform.position = transform.TransformPoint(pd.centrePos);
                pd.occupant = ot;
                return true;
            }
            return false;

        }

        // Create the mesh and position data. Does not
        // rely on Platform prefab. This is what is now
        // being used in the reworked code. Energy will 
        // be stored in active actors rather than the
        // platforms. Actors will be tracked by a table
        // storing all viable positions (level quads)
        public void CreateLandscapeMesh()
        {
            positionGrid.Clear();

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
            float yRange = landscapeHeight * .25f;

            // The virtual quads cover all vertices except zero, as there's
            // no adjacent vertex to take colour from, Same with last vertex
            // if it's an odd size, so add edge colour manually.
            Color edgeCol = useGradient ? gradient.Evaluate(0f) : defaultColor;
            colorArray[0] = edgeCol;
            if (xPanels % 2 != 0)
                colorArray[colorArray.Length - 1] = edgeCol;

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

                    Color col = useGradient ? gradient.Evaluate(closest) : defaultColor;

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
                        highestPointPanelIndex = quad.startTriangleIndex;
                    }
                    if (y < lowestPoint)
                    {
                        lowestPoint = y;
                        lowestPointPanelIndex = quad.startTriangleIndex;
                    }
                }

            }

            Debug.Log("Elapsed after calculating landing quads: " + (Time.realtimeSinceStartup - timer));
            timer = Time.realtimeSinceStartup;

            Debug.LogFormat("Highest index: {0} at {1}, Lowest index: {2} at {3}",
                highestPointPanelIndex, highestPoint, lowestPointPanelIndex, lowestPoint);

            Array.Copy(colorArray, colorBuffer, vertices.Length);

            //mesh.uv = uv; // Leave out if we're doing vertex colours

            mesh.vertices = vertices;
            mesh.tangents = tangents;
            mesh.triangles = triangles;
            mesh.colors32 = colorBuffer;

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            Debug.Log("Elapsed after adding landscape mesh: " + (Time.realtimeSinceStartup - timer));
            timer = Time.realtimeSinceStartup;

            var renderer = GetComponent<MeshRenderer>();
            if (renderer)
                renderer.enabled = true;
            var collider = GetComponent<MeshCollider>();
            if (collider)
                collider.sharedMesh = mesh;

            positionIndices = new int[positionGrid.Count];
            int pix = 0;
            foreach (var kv in positionGrid)
                positionIndices[pix++] = kv.Key;

            ready = true;
        }

        public int[] GetEmptyPositions()
        {
            List<int> indices = new List<int>(positionIndices);
            for (int i = 0; i < positionIndices.Length; i++)
            {
                PositionData pd = FindPositionData(positionIndices[i]);
                if (pd != null)
                {
                    if (pd.occupant == PositionData.OccupantType.None)
                        indices.Add(positionIndices[i]);
                    else
                        Debug.LogFormat("Skipped index {0} as it's not empty: contains {1}",
                            pd.landingQuad.startTriangleIndex, pd.occupant);
                }
                else
                    Debug.LogFormat("No data at index {0}", i);
            }

            return indices.ToArray();
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