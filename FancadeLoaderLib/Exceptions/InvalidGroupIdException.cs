﻿using System;
using System.Collections.Generic;
using System.Text;

namespace FancadeLoaderLib.Exceptions
{
    public class InvalidGroupIdException : Exception
    {
        public ushort ExpectedGroupId { get; private set; }
        public ushort PrefabGroupId { get; private set; }

        public InvalidGroupIdException(ushort expectedGroupId, ushort prefabGroupId)
            : base($"Prefab's group id ({prefabGroupId}) is differend than the group's id ({expectedGroupId})")
        {
            ExpectedGroupId = expectedGroupId;
            PrefabGroupId = prefabGroupId;
        }
    }
}