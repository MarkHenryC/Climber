using UnityEngine;

namespace QuiteSensible
{
    public class Triangle
    {
        public Vector3 v1, v2, v3;
    }

    /// <summary>
    /// This order and naming makes it easier to follow
    /// when we're converting landing data to a quad
    /// index. The quad is two tris starting bottom left
    /// </summary>
    public class LandingQuad
    {
        /**********************************

        Triangle as Quad layout

        P4_______P5
        P1       |
        |        |
        |________P2
        P0       P3
        
        ***********************************/

        public int ixBottomLeft, ixTopLeft, ixTopRight, ixBottomRight;
        public int startTriangleIndex;

        // A raycast hit may return the second triangle
        // of our virtual quads
        public static int StartTri(int tIndex)
        {
            if (tIndex % 2 != 0)
                tIndex--;

            return tIndex;
        }
    }
    /// <summary>
    /// Data for each quad (tri pair) that's been
    /// leveled out and can have actors placed on them
    /// </summary>
    public class PositionData
    {
        public enum OccupantType { None, Player, NPC, Object, Boss, Reserved };

        public Vector3 centrePos;
        public LandingQuad landingQuad;
        public OccupantType occupant = OccupantType.None;
    }

}