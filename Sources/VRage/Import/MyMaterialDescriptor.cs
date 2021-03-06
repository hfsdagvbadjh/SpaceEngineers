﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VRageMath;

namespace VRage.Import
{
    /// <summary>
    /// material params for export
    /// </summary>
    public class MyMaterialDescriptor
    {
        public string MaterialName { get; private set; }
        public Vector3 DiffuseColor = Vector3.One;
        public float SpecularPower;
        /// <summary>
        /// Extra data (animation of holos)
        /// </summary>
        public Vector3 ExtraData = Vector3.Zero;

        public float SpecularIntensity
        {
            get { return ExtraData.X; }
            set { ExtraData.X = value; }
        }

        public Dictionary<string, string> Textures = new Dictionary<string, string>();
        public Dictionary<string, string> UserData = new Dictionary<string, string>();

        public string Technique = "MESH";

        public string GlassCW = "";
        public string GlassCCW = "";
        public bool GlassSmoothNormals = true;

        /// <summary>
        /// c-tor
        /// </summary>
        /// <param name="materialName"></param>
        public MyMaterialDescriptor(string materialName)
        {
            MaterialName = materialName;
        }

        public MyMaterialDescriptor() {;}

        /// <summary>
        /// Write to binary file
        /// </summary>
        /// <param name="writer"></param>
        /// <returns></returns>
        public bool Write(BinaryWriter writer)
        {
            writer.Write((MaterialName != null) ? MaterialName : "");
            writer.Write(Textures.Count);
            foreach (var texture in Textures)
            {
                writer.Write(texture.Key);
                writer.Write(texture.Value == null ? "" : texture.Value);
            }

            writer.Write(UserData.Count);
            foreach (var userData in UserData)
            {
                writer.Write(userData.Key);
                writer.Write(userData.Value == null ? "" : userData.Value);
            }

            writer.Write(SpecularPower);
            writer.Write(DiffuseColor.X);
            writer.Write(DiffuseColor.Y);
            writer.Write(DiffuseColor.Z);
            writer.Write(ExtraData.X);
            writer.Write(ExtraData.Y);
            writer.Write(ExtraData.Z);

            writer.Write(Technique);

            if (Technique == "GLASS")
            {
                writer.Write(GlassCW);
                writer.Write(GlassCCW);
                writer.Write(GlassSmoothNormals);
            }

            return true;
        }

        public bool Read(BinaryReader reader, int version)
        {
            Textures.Clear();
            UserData.Clear();

            MaterialName = reader.ReadString();
            if (String.IsNullOrEmpty(MaterialName))
                MaterialName = null;

            if (version < 01052002)
            {
                var diffuseTextureName = reader.ReadString();
                if (!string.IsNullOrEmpty(diffuseTextureName))
                {
                    Textures.Add("DiffuseTexture", diffuseTextureName);
                }
                    
                var normalsTextureName = reader.ReadString();
                if (!string.IsNullOrEmpty(normalsTextureName))
                {
                    Textures.Add("NormalTexture", normalsTextureName);
                }
            }
            else
            {
                int texturesCount = reader.ReadInt32();
                for (int i = 0; i < texturesCount; i++)
                {
                    var textureName = reader.ReadString();
                    var texturePath = reader.ReadString();
                    Textures.Add(textureName, texturePath);
                }
            }

            if (version >= 01068001)
            {
                int userDataCount = reader.ReadInt32();
                for (int i = 0; i < userDataCount; i++)
                {
                    var name = reader.ReadString();
                    var data = reader.ReadString();
                    UserData.Add(name, data);
                }
            }

            SpecularPower = reader.ReadSingle();
            DiffuseColor.X = reader.ReadSingle();
            DiffuseColor.Y = reader.ReadSingle();
            DiffuseColor.Z = reader.ReadSingle();
            ExtraData.X = reader.ReadSingle();
            ExtraData.Y = reader.ReadSingle();
            ExtraData.Z = reader.ReadSingle();

            if (version < 01052001)
            {
                Technique = ((MyMeshDrawTechnique)reader.ReadInt32()).ToString();
            }
            else
                Technique = reader.ReadString();

            if (Technique == "GLASS")
            {
                if (version >= 01043001)
                {
                    GlassCW = reader.ReadString();
                    GlassCCW = reader.ReadString();
                    GlassSmoothNormals = reader.ReadBoolean();
                }
                else
                {
                    reader.ReadSingle();
                    reader.ReadSingle();
                    reader.ReadSingle();
                    reader.ReadSingle();
                    GlassCW = "GlassCW";
                    GlassCCW = "GlassCCW";
                    GlassSmoothNormals = false;
                }
            }

            return true;
        }
    }
}
