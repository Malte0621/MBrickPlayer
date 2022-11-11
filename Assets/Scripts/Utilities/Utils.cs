using BrickHill;
using System;
using System.Text.RegularExpressions;
using System.Text;
using UnityEngine;

namespace Utils {
    public static class Networking {
        public static byte[] GetPacketSize (int length) {
            if (length < 128) {
                byte[] s = new byte[1];
                s[0] = (byte)((length << 1) + 1);
                return s;
            } else if (length < 16512) {
                byte[] s = System.BitConverter.GetBytes((short)(((length - 128) << 2) + 2));
                return s;
            } else if (length < 2113664) {
                // um
            } else {
                // uh
            }
            return null;
        }
    }

    public static class Helper {
        public static string PrintBytes(byte[] byteArray)
        {
            var sb = new StringBuilder("<Buffer ");
            for(var i = 0; i < byteArray.Length;i++)
            {
                var b = byteArray[i];
                sb.AppendFormat("{0:x2} ", b);
            }
            sb.Append(">");
            return sb.ToString();
        }

        public static Vector3 SwapXY (this Vector3 vector) {
            return new Vector3(vector.y, vector.x, vector.z);
        }

        public static Vector3 SwapXZ (this Vector3 vector) {
            return new Vector3(vector.z, vector.y, vector.x);
        }

        public static Vector3 SwapYZ (this Vector3 vector) {
            return new Vector3(vector.x, vector.z, vector.y);
        }

        public static int ColorToDec (Color color) {
            // convert from 0-1 to 0-255
            int r = (int)(color.r * 255);
            int g = (int)(color.g * 255);
            int b = (int)(color.b * 255);

            // do weird stuff i cant even
            return (r << 16) + (g << 8) + b;
        }

        public static Color DecToColor (int decimalColor, bool bgr = false) {
            int r = (decimalColor >> 16) & 0xff;
            int g = (decimalColor >> 8) & 0xff;
            int b = decimalColor & 0xff;
            if (bgr)
            {
                return new Color(b/255f, g/255f, r/255f); // divide by 255 to convert 0-255 values to 0-1
            }
            else
            {
                return new Color(r / 255f, g / 255f, b / 255f); // divide by 255 to convert 0-255 values to 0-1
            }
        }

        public static int Mod(this int a, int b) {
            return (a % b + b) % b; // % operator is actually remainder and not modulo which is fine for positive numbers but not negative numbers, so this function is necessary when potentially working with negative numbers
        }

        // Vector3 Damping, for translations
        public static Vector3 Damp(Vector3 a, Vector3 b, float t, float dt)
        {
            return Vector3.Lerp(a, b, 1 - Mathf.Pow(t, dt));
        }

        // Quaternion Damping, for rotations
        public static Quaternion Damp(Quaternion a, Quaternion b, float t, float dt)
        {
            return Quaternion.Slerp(a, b, 1 - Mathf.Pow(t, dt));
        }

        public static Color BGR (this Color color) {
            return new Color(color.b, color.g, color.r, color.a);
        }

        public static void PlaySound (AudioClip sound, float volume, float pitch) {
            GameObject sfx = new GameObject("Sound Effect");
            AudioSource audioSource = sfx.AddComponent<AudioSource>();
            audioSource.volume = volume;
            audioSource.pitch = pitch;
            audioSource.spatialBlend = 0f;
            audioSource.clip = sound;
            audioSource.Play();
            MonoBehaviour.Destroy(sfx, sound.length);
        }

        public static Map.Brick.ShapeType GetShapeFromName (string name) {
            if (Enum.TryParse<Map.Brick.ShapeType>(name.ToLower(), out Map.Brick.ShapeType shape)) {
                return shape;
            }
            return Map.Brick.ShapeType.cube;
        }

        public static Vector3 CorrectBHScale (Vector3 scale, int rotation) {
            if (rotation == 0 || rotation == 180) {
                return scale;
            }
            return scale.SwapXY();
        }

        public static Vector3 BigVector3 = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);

        public static Vector3 Clamp (this Vector3 input, Vector3 min, Vector3 max) {
            return new Vector3(
                input.x > max.x ? max.x : input.x < min.x ? min.x : input.x,
                input.y > max.y ? max.y : input.y < min.y ? min.y : input.y,
                input.z > max.z ? max.z : input.z < min.z ? min.z : input.z
            );
        }

        public static Vector3 NotQuiteZeroButClose (this Vector3 input) {
            float small = 0.0001f;
            return new Vector3(
                input.x == 0 ? small : input.x,
                input.y == 0 ? small : input.y,
                input.z == 0 ? small : input.z
            );
        }

        public static Vector3 Round (this Vector3 input, float nearest) {
            return new Vector3(
                Mathf.Round(input.x * nearest) / nearest,
                Mathf.Round(input.y * nearest) / nearest,
                Mathf.Round(input.z * nearest) / nearest
            );
        }


        public static string FilteredText(string input) {
            string hexColorCodes = input.Replace("<color:", "<color=#");

            // bh is bad and uses BGR so we need to flip all the colors now gg
            Regex hexColor = new Regex("#(?:[0-9a-fA-F]{6})");
            MatchCollection matches = hexColor.Matches(hexColorCodes);
            for (int i = 0; i < matches.Count; i++) {
                string original = matches[i].Value;
                string reversed = original.Substring(1);
                if (reversed.Length == 6) {
                    string temp = "";
                    temp += reversed.Substring(4);
                    temp += reversed.Substring(2, 2);
                    temp += reversed.Substring(0, 2);
                    reversed = temp;

                    hexColorCodes = hexColorCodes.Replace(original, "#" + reversed);
                }
            }

            string white = hexColorCodes.Replace(@"\c0", "<color=#FFFFFF>");
            string lgray = white.Replace(@"\c1", "<color=#AAAAAA>");
            string dgray = lgray.Replace(@"\c2", "<color=#555555>");
            string black = dgray.Replace(@"\c3", "<color=#000000>");
            string blue = black.Replace(@"\c4", "<color=#0000FF>");
            string green = blue.Replace(@"\c5", "<color=#00FF00>");
            string red = green.Replace(@"\c6", "<color=#FF0000>");
            string cyan = red.Replace(@"\c7", "<color=#00FFFF>");
            string yellow = cyan.Replace(@"\c8", "<color=#FFFF00>");
            string magenta = yellow.Replace(@"\c9", "<color=#FF00FF>");

            return magenta;
        }

        public static Vector3 RotatePointAroundPivot (Vector3 pivot, Vector3 point, Vector3 rotation) {
            return Quaternion.Euler(rotation) * (point - pivot) + pivot;
        }
    }
}
