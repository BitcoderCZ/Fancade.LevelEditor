﻿using System;
using System.Collections.Generic;
using System.Text;

namespace FancadeLoaderLib
{
    public class Game
    {
        private string name;
        public string Name
        {
            get => name;
            set
            {
                if (value is null)
                    throw new ArgumentNullException(nameof(value), $"{nameof(Name)} cannot be null.");

                name = value;
            }
        }
        private string author;
        public string Author
        {
            get => author;
            set
            {
                if (value is null)
                    throw new ArgumentNullException(nameof(value), $"{nameof(Author)} cannot be null.");

                author = value;
            }
        }
        private string description;
        public string Description
        {
            get => description;
            set
            {
                if (value is null)
                    throw new ArgumentNullException(nameof(value), $"{nameof(Description)} cannot be null.");

                description = value;
            }
        }

        public readonly List<Prefab> Prefabs;

        public Game(string name, string author, string description, List<Prefab> prefabs)
        {
            if (prefabs is null)
                throw new ArgumentNullException(nameof(prefabs));

            Name = name;
            Author = author;
            Description = description;
            Prefabs = prefabs;
        }

        public void MakeEditable(bool changeAuthor)
        {
            if (changeAuthor)
                Author = "Unknown Author";

        }

        public RawGame ToRaw(bool clonePrefabs)
        {
            List<RawPrefab> prefabs = new List<RawPrefab>(Prefabs.Count);

            for (int i = 0; i < Prefabs.Count; i++)
                prefabs.Add(Prefabs[i].ToRaw(clonePrefabs));

            return new RawGame(Name, Author, Description, RawGame.CurrentNumbStockPrefabs, prefabs);
        }

        public static Game FromRaw(RawGame game, bool clonePrefabs = true)
        {
            List<Prefab> prefabs = new List<Prefab>(game.Prefabs.Count);

            short idOffsetAddition = (short)(-game.IdOffset + RawGame.CurrentNumbStockPrefabs);

            for (int i = 0; i < game.Prefabs.Count; i++)
                prefabs.Add(Prefab.FromRaw(game.Prefabs[i], game.IdOffset, idOffsetAddition, clonePrefabs));

            return new Game(game.Name, game.Author, game.Description, prefabs);
        }
    }
}
