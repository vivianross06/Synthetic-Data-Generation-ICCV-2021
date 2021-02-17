using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using System.Linq;

public class PlyLoader
{
    public PlyLoader()
    {
    }

    enum Format
    {
        ascii,
        binary_little_endian,
        binary_big_endian
    };


    Format getFormat(string s)
    {
        switch (s)
        {
            case "ascii": return Format.ascii;
            case "binary_little_endian": return Format.binary_little_endian;
            //case "binary_big_endian": return Format.binary_big_endian;
            default: throw (new Exception("bad PLY format " + s + " (currently only binary_little_endian and ascii supported"));
        }
    }

    enum NumFormat
    {
        nf_uchar,
        nf_ushort,
        nf_uint,
        nf_char,
        nf_short,
        nf_int,
        nf_float,
    };

    NumFormat getNumFormat(string s)
    {
        switch (s)
        {
            case "uchar": return NumFormat.nf_uchar;
            case "ushort": return NumFormat.nf_ushort;
            case "uint": return NumFormat.nf_uint;
            case "char": return NumFormat.nf_char;
            case "short": return NumFormat.nf_short;
            case "int": return NumFormat.nf_int;
            case "float": return NumFormat.nf_float;
            default: throw (new Exception("bad PLY format " + s));
        }
    }

    private int[] semanticColors;

    private void loadSemanticColors() {

        //semanticColors = new Dictionary<int, int>();

        //Instead of parsing the .tsv files every time, parse once at the beginning and use a dictionary map category ids to semantic colors.

        string catfilename = Application.dataPath + "/MatterportMetadata/category_mapping.tsv";
        string colfilname = Application.dataPath + "/MatterportMetadata/mpcat40.tsv";

        List<int> sc = new List<int>();
        sc.Add(0); //Default Color = black

        String[] content = File.ReadAllLines(catfilename);

        var catls = new List<List<string>>();

        for (int i = 0; i < content.Length; i++)
        {
            catls.Add((content[i].Split('\t')).ToList());
        }

        content = File.ReadAllLines(colfilname);

        var colls = new List<List<string>>();
        for (int i = 0; i < content.Length; i++)
        {
            colls.Add((content[i].Split('\t')).ToList());
        }

        int mp40col = catls[0].IndexOf("mpcat40index");
        int colcol = colls[0].IndexOf("hex");

        for (int i = 1; i < catls.Count; i++) {
            int mp40Index = Int32.Parse(catls[i][mp40col]);
            string hexCol = colls[mp40Index + 1][colcol];
            int intValue = int.Parse(hexCol.Substring(1), System.Globalization.NumberStyles.HexNumber);
            sc.Add(intValue);
        }
        semanticColors = sc.ToArray();
        

    }

    public Mesh[] load(string filename)
    {
        loadSemanticColors();
        if (File.Exists(filename))
        {
            using (FileStream file = File.Open(filename, FileMode.Open))
            {
                // Parse the ASCII text at the start up to "end_header\n"
                bool done = false;
                Format fmt = Format.ascii;
                int num_vertices = 0;
                int num_faces = 0;
                int num_vertex_props = 0;
                int num_face_props = 0;
                //vertices
                List<NumFormat> prop_types = new List<NumFormat>();
                List<string> prop_names = new List<string>();
                List<float[]> prop_values = new List<float[]>();
                //faces
                List<NumFormat> face_prop_types = new List<NumFormat>();
                List<string> face_prop_names = new List<string>();
                List<int[]> face_prop_values = new List<int[]>();

                string cur_element = "";

                int prop_x = -1;
                int prop_y = -1;
                int prop_z = -1;
                int prop_nx = -1;
                int prop_ny = -1;
                int prop_nz = -1;
                int prop_u = -1;
                int prop_v = -1;
                int prop_r = -1;
                int prop_g = -1;
                int prop_b = -1;
                int prop_a = -1;
      
                int face_prop_material = -1;
                int face_prop_segment = -1;
                int face_prop_category = -1;

                while (!done)
                {
                    StringBuilder builder = new StringBuilder("");
                    int b;
                    for (; ; )
                    {
                        b = file.ReadByte();
                        if (b == '\n') break;
                        builder.Append((char)b);
                    }
                    string line = builder.ToString();
                    string[] words = line.Split(' ');
                    switch (words[0])
                    {
                        case "ply":
                            {
                            }
                            break;
                        case "comment":
                            {
                            }
                            break;
                        case "format":
                            {
                                fmt = getFormat(words[1]);
                            }
                            break;
                        case "element":
                            {
                                cur_element = words[1];
                                if (words[1] == "vertex")
                                {
                                    num_vertices = int.Parse(words[2]);
                                }
                                else if (words[1] == "face")
                                {
                                    num_faces = int.Parse(words[2]);
                                }
                                else
                                {
                                    throw (new Exception("bad PLY element"));
                                }
                            }
                            break;
                        case "property":
                            {
                                if (cur_element == "vertex")
                                {
                                    prop_types.Add(getNumFormat(words[1]));
                                    prop_names.Add(words[2]);
                                    switch (words[2])
                                    {
                                        case "x": prop_x = num_vertex_props; break;
                                        case "y": prop_y = num_vertex_props; break;
                                        case "z": prop_z = num_vertex_props; break;
                                        case "nx": prop_nx = num_vertex_props; break;
                                        case "ny": prop_ny = num_vertex_props; break;
                                        case "nz": prop_nz = num_vertex_props; break;
                                        case "u": prop_u = num_vertex_props; break;
                                        case "v": prop_v = num_vertex_props; break;
                                        case "red": prop_r = num_vertex_props; break;
                                        case "green": prop_g = num_vertex_props; break;
                                        case "blue": prop_b = num_vertex_props; break;
                                        case "alpha": prop_a = num_vertex_props; break;
                                    }
                                    num_vertex_props++;
                                }
                                else if (cur_element == "face")
                                {
                                    if (line != "property list uchar int vertex_indices")
                                    {
                                        face_prop_types.Add(getNumFormat(words[1]));
                                        face_prop_names.Add(words[2]);
                                        switch (words[2])
                                        {
                                            case "material_id": face_prop_material = num_face_props; break;
                                            case "segment_id":  face_prop_segment = num_face_props; break;
                                            case "category_id": face_prop_category = num_face_props; break;
                                        }
                                        num_face_props++;
                                    }
                                }
                            }
                            break;
                        case "end_header":
                            {
                                done = true;
                            }
                            break;
                        default:
                            {
                                throw (new Exception("bad PLY field"));
                            };
                    }
                }

                int num_props = prop_names.Count;
                int num_props_faces = face_prop_names.Count;

                //Debug.Log("" + num_vertices + " vertices " + num_faces  + " faces " + num_props + " props!");

                for (int i = 0; i != num_props; ++i)
                {
                    prop_values.Add(new float[num_vertices]);
                }
                for (int i = 0; i < num_props_faces; i++)
                {
                    face_prop_values.Add(new int[num_faces]);
                }

                BinaryReader reader = null;
                StreamReader streamReader = null;
                if (fmt == Format.ascii)
                {
                    streamReader = new StreamReader(file);
                }
                else if (fmt == Format.binary_little_endian)
                {
                    reader = new BinaryReader(file);
                }

                for (int i = 0; i != num_vertices; ++i)
                {
                    if (fmt == Format.ascii)
                    {
                        string line = streamReader.ReadLine();
                        // Note: assumes values are space-separated which may not be the case.
                        string[] values = line.Split(' ');
                        for (int j = 0; j != num_props; ++j)
                        {
                            switch (prop_types[j])
                            {
                                case NumFormat.nf_uchar: prop_values[j][i] = (byte)Int32.Parse(values[j]) * (1.0f / 255); break;
                                case NumFormat.nf_ushort: prop_values[j][i] = (ushort)Int32.Parse(values[j]) * (1.0f / 65535); break;
                                case NumFormat.nf_uint: prop_values[j][i] = (uint)Int32.Parse(values[j]) * (1.0f / 0xffffffff); break;
                                case NumFormat.nf_char: prop_values[j][i] = (sbyte)Int32.Parse(values[j]) * (1.0f / 127); break; // not absolutely correct
                                case NumFormat.nf_short: prop_values[j][i] = (short)Int32.Parse(values[j]) * (1.0f / 32767); break;
                                case NumFormat.nf_int: prop_values[j][i] = Int32.Parse(values[j]) * (1.0f / 0x7fffffff); break;
                                case NumFormat.nf_float: prop_values[j][i] = Single.Parse(values[j]); break;
                            }
                        }
                    }
                    else if (fmt == Format.binary_little_endian)
                    {
                        for (int j = 0; j != num_props; ++j)
                        {
                            switch (prop_types[j])
                            {
                                case NumFormat.nf_uchar: prop_values[j][i] = (byte)reader.ReadByte() * (1.0f / 255); break;
                                case NumFormat.nf_ushort: prop_values[j][i] = (ushort)reader.ReadInt16() * (1.0f / 65535); break;
                                case NumFormat.nf_uint: prop_values[j][i] = (uint)reader.ReadInt32() * (1.0f / 0xffffffff); break;
                                case NumFormat.nf_char: prop_values[j][i] = (sbyte)reader.ReadByte() * (1.0f / 127); break; // not absolutely correct
                                case NumFormat.nf_short: prop_values[j][i] = (short)reader.ReadInt16() * (1.0f / 32767); break;
                                case NumFormat.nf_int: prop_values[j][i] = reader.ReadInt32() * (1.0f / 0x7fffffff); break;
                                case NumFormat.nf_float: prop_values[j][i] = reader.ReadSingle(); break;
                            }
                        }
                    }
                }

                bool no_xyz = (prop_x == -1 || prop_y == -1 || prop_z == -1);
                bool no_nxyz = (prop_nx == -1 || prop_ny == -1 || prop_nz == -1);
                bool no_uv = (prop_u == -1 || prop_v == -1);
                bool no_rgb = (prop_r == -1 || prop_g == -1 || prop_b == -1);

                if (no_xyz)
                {
                    throw (new Exception("bad PLY no xyz"));
                }

                List<Mesh> meshes = new List<Mesh>();
                Dictionary<int, int> global_to_local;

                // local values (limited to 64k vertices)
                int num_local = 0;
                List<int> indices;
                List<Vector3> vertices;
                List<Color32> colors;
                List<Vector3> normals;
                List<Vector2> uvs;

                {
                    indices = new List<int>();
                    global_to_local = new Dictionary<int, int>();
                    vertices = new List<Vector3>();
                    colors = new List<Color32>();
                    normals = new List<Vector3>();
                    uvs = new List<Vector2>();
                    num_local = 0;
                }
                for (int i = 0; i != num_faces; ++i)
                {
                    int nj = 0; //uchar
                    int a = 0;
                    int b = 0;
                    string[] numbers = null;
                    int numbers_idx = 0;
                    List<int> face_vertices = new List<int>();

                    if (fmt == Format.ascii)
                    {
                        numbers = streamReader.ReadLine().Split(' ');
                        nj = Int32.Parse(numbers[0]);
                        a = Int32.Parse(numbers[1]);
                        b = Int32.Parse(numbers[2]);
                        numbers_idx = 3;
                    }
                    else
                    {
                        nj = reader.ReadByte();
                        a = reader.ReadInt32();
                        b = reader.ReadInt32();

                    }
                    face_vertices.Add(a);
                    face_vertices.Add(b);
                    for (int j = 2; j < nj; ++j)
                    {
                        int c = fmt == Format.ascii ? Int32.Parse(numbers[numbers_idx++]) : reader.ReadInt32();
                        face_vertices.Add(c);
                        int[] abc = { a, b, c };
                        foreach (int g in abc)
                        {
                            // translate from global to local indices (64k limit)
                            if (global_to_local.ContainsKey(g))
                            {
                                indices.Add(global_to_local[g]);
                            }
                            else
                            {
                                List<float[]> pv = prop_values;
                                vertices.Add(new Vector3(pv[prop_x][g], pv[prop_y][g], pv[prop_z][g]));
                                if (!no_nxyz) normals.Add(new Vector3(pv[prop_nx][g], pv[prop_ny][g], pv[prop_nz][g]));
                                if (!no_uv) uvs.Add(new Vector2(pv[prop_u][g], pv[prop_v][g]));
                                else uvs.Add(new Vector2(0, 0));
                                if (!no_rgb) colors.Add(new Color32((byte)(pv[prop_r][g] * 255.0f), (byte)(pv[prop_g][g] * 255.0f), (byte)(pv[prop_b][g] * 255.0f), 255));
                                global_to_local[g] = num_local;
                                indices.Add(num_local++);
                            }
                        }
                        b = c;
                    }
                    for (int j = 0; j < num_face_props; j++)
                    {
                        if (fmt == Format.ascii)
                        {
                            string line = streamReader.ReadLine();
                            // Note: assumes values are space-separated which may not be the case.
                            string[] values = line.Split(' ');
                            /*
                            switch (face_prop_types[j])
                            {
                                case NumFormat.nf_uchar: face_prop_values[j][i] = (byte)Int32.Parse(values[j]); break;
                                case NumFormat.nf_ushort: face_prop_values[j][i] = (ushort)Int32.Parse(values[j]); break;
                                case NumFormat.nf_uint: face_prop_values[j][i] = (uint)Int32.Parse(values[j]); break;
                                case NumFormat.nf_char: face_prop_values[j][i] = (sbyte)Int32.Parse(values[j]); break; // not absolutely correct
                                case NumFormat.nf_short: face_prop_values[j][i] = (short)Int32.Parse(values[j]); break;
                                case NumFormat.nf_int: face_prop_values[j][i] = Int32.Parse(values[j]); break;
                                case NumFormat.nf_float: face_prop_values[j][i] = Single.Parse(values[j]); break;
                            }
                            */
                            face_prop_values[j][i] = Int32.Parse(values[j]);
                        }
                        else if (fmt == Format.binary_little_endian)
                        {
                            /*
                            switch (face_prop_types[j])
                            {
                                case NumFormat.nf_uchar: face_prop_values[j][i] = (byte)reader.ReadByte(); break;
                                case NumFormat.nf_ushort: face_prop_values[j][i] = (ushort)reader.ReadInt16(); break;
                                case NumFormat.nf_uint: face_prop_values[j][i] = (uint)reader.ReadInt32(); break;
                                case NumFormat.nf_char: face_prop_values[j][i] = (sbyte)reader.ReadByte(); break; // not absolutely correct
                                case NumFormat.nf_short: face_prop_values[j][i] = (short)reader.ReadInt16(); break;
                                case NumFormat.nf_int: face_prop_values[j][i] = reader.ReadInt32(); break;
                                case NumFormat.nf_float: face_prop_values[j][i] = (int)reader.ReadSingle(); break;
                            }
                            */
                            face_prop_values[j][i] = reader.ReadInt32();
                        }
                    }

                    for (int k = 0; k < face_vertices.Count; k++)
                    {
                        //Debug.Log(face_prop_values[2][i]);
                        if (!(face_prop_values[2][i]  < 0))
                            uvs[global_to_local[face_vertices[k]]] = new Vector2(semanticColors[face_prop_values[2][i]], 0);
                        else
                            uvs[global_to_local[face_vertices[k]]] = new Vector2(0, 0);
                    }

                    if (num_local >= 65530 || i + 1 == num_faces)
                    {
                        Mesh mesh = new Mesh();
                        meshes.Add(mesh);
                        //Debug.Log("vertices=" + vertices.Count + " normals=" + normals.Count + " uvs=" + uvs.Count + " colors=" + colors.Count);
                        mesh.vertices = vertices.ToArray();
                        mesh.uv = uvs.ToArray(); //if (!no_uv) 
                        if (!no_nxyz) mesh.normals = normals.ToArray();
                        if (!no_rgb) mesh.colors32 = colors.ToArray();
                        mesh.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0, true);
                        mesh.name = "mesh";
                        indices = new List<int>();
                        global_to_local = new Dictionary<int, int>();
                        vertices = new List<Vector3>();
                        colors = new List<Color32>();
                        normals = new List<Vector3>();
                        uvs = new List<Vector2>();
                        num_local = 0;
                    }
                }
                return meshes.ToArray();
            }
        }
        throw (new Exception("file doesn't exist, check path"));
        //return new Mesh[0];
    }
}
