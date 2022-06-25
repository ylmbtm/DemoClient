using UnityEngine;
using System.Collections;

namespace ACT
{
    public class ActUtility
    {
        public static bool IsInOBB(Actor target, float length, float width, float height, Vector3 hitPos, Vector3 hitDir)
        {
            float y = hitPos.y;
            float yMax = y + height / 2;
            float yMin = y - height / 2;

            if (target.Pos.y + target.Height <= yMin)
            {
                return false;
            }
            if (target.Pos.y >= yMax)
            {
                return false;
            }

            Quaternion q = Quaternion.FromToRotation(Vector3.forward, hitDir);
            Vector3 P = target.Pos;
            Vector3 A = q * new Vector3(-width / 2, 0, 0)      + hitPos;
            Vector3 B = q * new Vector3(-width / 2, 0, length) + hitPos;
            Vector3 C = q * new Vector3( width / 2, 0, length) + hitPos;
            Vector3 D = q * new Vector3( width / 2, 0, 0)      + hitPos;


            if (Vector3.Dot(P - A, B - A) >= 0                         && 
                Vector3.Dot(P - A, B - A) <= Vector3.Dot(B - A, B - A) &&
                Vector3.Dot(P - A, D - A) >= 0                         && 
                Vector3.Dot(P - A, D - A) <= Vector3.Dot(D - A, D - A))
            {
                return true;
            }
            if (IsLineSegmentIntersectCircle(B, A, P, target.Radius))
            {
                return true;
            }
            if (IsLineSegmentIntersectCircle(A, D, P, target.Radius))
            {
                return true;
            }
            if (IsLineSegmentIntersectCircle(D, C, P, target.Radius))
            {
                return true;
            }
            if (IsLineSegmentIntersectCircle(C, B, P, target.Radius))
            {
                return true;
            }
            return false;
        }

        public static bool IsInOBBByCenter(Actor target, float length, float width, float height, Vector3 center, Vector3 hitDir)
        {
            float y = center.y;
            float yMax = y + height / 2;
            float yMin = y - height / 2;

            if (target.Pos.y + target.Height <= yMin)
            {
                return false;
            }
            if (target.Pos.y >= yMax)
            {
                return false;
            }

            Quaternion q = Quaternion.FromToRotation(Vector3.forward, hitDir);
            Vector3 P = target.Pos;
            Vector3 A = q * new Vector3(-width / 2, 0, -length / 2) + center;
            Vector3 B = q * new Vector3(-width / 2, 0,  length / 2) + center;
            Vector3 C = q * new Vector3(width / 2, 0,   length / 2) + center;
            Vector3 D = q * new Vector3(width / 2, 0,  -length / 2) + center;


            if (Vector3.Dot(P - A, B - A) >= 0 &&
                Vector3.Dot(P - A, B - A) <= Vector3.Dot(B - A, B - A) &&
                Vector3.Dot(P - A, D - A) >= 0 &&
                Vector3.Dot(P - A, D - A) <= Vector3.Dot(D - A, D - A))
            {
                return true;
            }
            if (IsLineSegmentIntersectCircle(B, A, P, target.Radius))
            {
                return true;
            }
            if (IsLineSegmentIntersectCircle(A, D, P, target.Radius))
            {
                return true;
            }
            if (IsLineSegmentIntersectCircle(D, C, P, target.Radius))
            {
                return true;
            }
            if (IsLineSegmentIntersectCircle(C, B, P, target.Radius))
            {
                return true;
            }
            return false;
        }

        public static bool IsLineSegmentIntersectCircle(Vector3 start, Vector3 end, Vector3 P, float radius)
        {
            float sqrDistance = (GTTools.GetClosestPointFromLineSegmentToPoint(start, end, P) - P).sqrMagnitude;
            return sqrDistance < radius * radius;
        }

        public static bool IsInAABB(Actor target, float length, float width, float height, Vector3 hitPos)
        {
            Vector3 cirPos = target.Pos;
            Vector3 p = cirPos;
            Vector3 c = hitPos;
            p.y = 0;
            c.y = 0;
            Vector3 v = Vector3.Max(p - c, c - p);
            Vector3 h = new Vector3(width / 2, 0, length / 2);
            Vector3 u = Vector3.Max(v - h, Vector3.zero);
            return Vector3.Dot(u, u) <= (target.Radius * target.Radius);
        }

        public static bool IsInSector(Actor target, float radius, float hAngle, float height, Vector3 hitPoint, Vector3 hitDir)
        {
            float y = hitPoint.y;
            float yMax = y + height / 2;
            float yMin = y - height / 2;
            if (target.Pos.y + target.Height <= yMin)
            {
                return false;
            }
            if (target.Pos.y >= yMax)
            {
                return false;
            }

            float maxDis = radius + target.Radius;
            Vector3 tarDir = hitPoint - target.Pos;
            tarDir.y = 0;
            if (tarDir.sqrMagnitude > maxDis * maxDis)
            {
                return false;
            }
            Vector3 targetPos = target.Pos;
            targetPos.y = 0;
            Vector3 centerPos = hitPoint;
            centerPos.y = 0;

            Vector3 srcDir = hitDir;
            srcDir.y = 0;
            if (Vector3.Angle(targetPos - centerPos, srcDir) > hAngle / 2)
            {
                return false;
            }
            return true;
        }

        public static bool IsInCircle(Actor target, float radius, float height, Vector3 hitPoint)
        {
            float y = hitPoint.y;
            float yMax = y + height / 2;
            float yMin = y - height / 2;
            if (target.Pos.y + target.Height <= yMin)
            {
                return false;
            }
            if (target.Pos.y >= yMax)
            {
                return false;
            }
            Vector3 dir = target.Pos - hitPoint;
            dir.y = 0;
            if(dir.sqrMagnitude > radius + target.Radius)
            {
                return false;
            }
            return true;
        }

        public static bool IsInSphere(Actor target, float radius, Vector3 hitPoint)
        {
            float y = hitPoint.y;
            float yMax = y + radius;
            float yMin = y - radius;
            if (target.Pos.y + target.Height <= yMin)
            {
                return false;
            }
            if (target.Pos.y >= yMax)
            {
                return false;
            }

            Vector3 dir = target.Pos - hitPoint;
            dir.y = 0;
            float dis = radius + target.Radius;
            if (dir.sqrMagnitude > dis * dis)
            {
                return false;
            }
            return true;
        }
    }
}