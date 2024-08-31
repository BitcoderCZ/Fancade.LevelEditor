﻿using FancadeLoaderLib.Exceptions;
using MathUtils.Vectors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace FancadeLoaderLib
{
    public class RawGame
    {
        public const ushort OldestBlockPaletteVersion = 27;
        public static readonly ushort CurrentFileVersion = 31;
        public static readonly ushort CurrentNumbStockPrefabs = 597;

        private string Name;
        private string Author;
        private string Description;

        public ushort IdOffset;

        public readonly List<RawPrefab> Prefabs;

        public RawGame(string _name)
        {
            if (_name is null) throw new ArgumentNullException(nameof(_name));

            Name = _name;
            Author = "Unknown Author";
            Description = string.Empty;
            Prefabs = new List<RawPrefab>();
        }

        private RawGame(string _name, string _author, string _description, ushort _idOffset, List<RawPrefab> _prefabs)
        {
            Name = _name;
            Author = _author;
            Description = _description;
            IdOffset = _idOffset;
            Prefabs = _prefabs;
        }

        public void SaveCompressed(Stream stream)
        {
            using (MemoryStream writerStream = new MemoryStream())
            using (FcBinaryWriter writer = new FcBinaryWriter(writerStream))
            {
                Save(writer);

                Zlib.Compress(writerStream, stream);
            }
        }
        public void Save(FcBinaryWriter writer)
        {
            writer.WriteUInt16(CurrentFileVersion);
            writer.WriteString(Name);
            writer.WriteString(Author);
            writer.WriteString(Description);
            writer.WriteUInt16(CurrentNumbStockPrefabs);

            writer.WriteUInt16((ushort)Prefabs.Count);

            for (int i = 0; i < Prefabs.Count; i++)
                Prefabs[i].Save(writer);
        }

        public static (ushort FileVersion, string Name, string Author, string Description) LoadInfoCompressed(Stream stream)
        {
            // decompress
            FcBinaryReader reader = new FcBinaryReader(new MemoryStream());
            Zlib.Decompress(stream, reader.Stream);
            reader.Reset();

            ushort fileVersion = reader.ReadUInt16();

            string name = reader.ReadString();
            string author = reader.ReadString();
            string description = reader.ReadString();

            return (fileVersion, name, author, description);
        }

        public static RawGame LoadCompressed(Stream stream)
        {
            // decompress
            FcBinaryReader reader = new FcBinaryReader(new MemoryStream());
            Zlib.Decompress(stream, reader.Stream);
            reader.Reset();

            return Load(reader);
        }
        public static RawGame Load(FcBinaryReader reader)
        {
            ushort fileVersion = reader.ReadUInt16();

            if (fileVersion > CurrentFileVersion || fileVersion < 26)
                throw new UnsupportedVersionException(fileVersion);
            else if (fileVersion == 26)
                throw new NotImplementedException("Loading file verison 26 has not yet been implemented.");

            string name = reader.ReadString();
            string author = reader.ReadString();
            string description = reader.ReadString();

            ushort idOffset = reader.ReadUInt16();

            ushort numbPrefabs = reader.ReadUInt16();

            List<RawPrefab> prefabs = new List<RawPrefab>(numbPrefabs);
            for (int i = 0; i < numbPrefabs; i++)
                prefabs.Add(RawPrefab.Load(reader));

            return new RawGame(name, author, description, idOffset, prefabs);
        }

        /// <summary>
        /// Sets Uneditable to !<paramref name="editable"/> on <see cref="CustomBlocks"/> and <see cref="Levels"/>
        /// </summary>
        /// <param name="editable">If this <see cref="RawGame"/> should be editable or not</param>
        /// <param name="changeAuthor">If this and <paramref name="editable"/> are both <see href="true"/>, <see cref="Author"/> gets set to "Unknown Author"</param>
        public void SetEditable(bool editable, bool changeAuthor)
        {
            bool b = !editable;
            if (editable && changeAuthor)
                Author = "Unknown Author";

            CustomBlocks.EnumerateBlocks(item =>
            {
                item.Value.Attribs.Uneditable = b;
                foreach (KeyValuePair<Vector3I, BlockSection> item2 in item.Value.Sections)
                    item2.Value.Attribs.Uneditable = b;
            });

            foreach (Level level in Levels)
                level.Uneditable = b;
        }

        public override string ToString() => $"[Name: {Name}, Author: {Author}, Description: {Description}]";
    }
}