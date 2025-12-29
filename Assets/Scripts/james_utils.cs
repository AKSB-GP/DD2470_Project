using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

namespace  james_utils {


    public static class IListExtensions {
        /// <summary>
        /// Shuffles the element order of the specified list.
        /// </summary>
        public static void Shuffle<T>(this IList<T> ts) {
            var count = ts.Count;
            var last = count - 1;
            for (var i = 0; i < last; ++i) {
                var r = UnityEngine.Random.Range(i, count);
                var tmp = ts[i];
                ts[i] = ts[r];
                ts[r] = tmp;
            }
        }
    }

    public static class csv_utils {

        public static List<Vector3> CSVRead2Vector3List (List<Vector3> stimSP, StreamReader reader) {
            using (reader) {
                while (!reader.EndOfStream) {
                    var line = reader.ReadLine ();
                    var values = line.Split (',');

                    stimSP.Add (new Vector3 (
                        float.Parse (values[0], CultureInfo.InvariantCulture),
                        float.Parse (values[1], CultureInfo.InvariantCulture),
                        float.Parse (values[2], CultureInfo.InvariantCulture)));
                }
            }
            return stimSP;
        }


        public static void Log (string[] values, StreamWriter writer_, bool isLogging) {
            if (!isLogging || writer_ == null)
                return;

            string line = "";
            for (int i = 0; i < values.Length; ++i) {
                //Debug.Log ("value[i]: " + values[i].ToString ());
                values[i] = values[i].Replace ("\r", "").Replace ("\n", ""); // Remove new lines so they don't break csv
                line += values[i] + (i == (values.Length - 1) ? "" : ";"); // add semicolon to all but the last data string
            }
            writer_.WriteLine (line);
        }
    }

}