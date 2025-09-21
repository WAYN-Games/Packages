// /*
//  * Copyright (c) 2025 WAYN Games.
//  *
//  * All rights reserved. This software and associated documentation files
//  * (the "Software") are the exclusive property of WAYN Games.
//  * Unauthorized copying of this file, via any medium, is strictly prohibited.
//  *
//  * This software may not be reproduced, distributed, or used in any manner
//  * whatsoever without the express written permission of the author, except for
//  * the use of brief quotations in a review or other non-commercial uses permitted
//  * by copyright law.
//  *
//  * For permissions, contact: contact@wayn.games
//  */

using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.Editor)]
public partial struct EntityGoEditorPreviewSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (prefab, localToWorld) in SystemAPI.Query<GameObjectEntity, RefRO<LocalToWorld>>())
        {
            GameObject go = prefab.Prefab;
            if (go == null) continue;

            var list = new List<MeshPreview>();

            Mesh mesh;
            Material[] mats;
            var mfs = go.GetComponentsInChildren<MeshFilter>();
            foreach (MeshFilter mf in mfs)
            {
                mesh = mf.sharedMesh;
                mats = mf.gameObject.GetComponentInChildren<MeshRenderer>().sharedMaterials;
                if (mats == null) continue;
                Vector3 position = mf.transform.position + (Vector3)localToWorld.ValueRO.Position;
                Quaternion rotation = mf.transform.rotation * localToWorld.ValueRO.Rotation;
                Vector3 scale = mf.transform.localScale;
                list.Add(new MeshPreview
                {
                    Mesh = mesh,
                    Materials = mats,
                    Position = position,
                    Rotation = rotation,
                    Scale = scale
                });
            }

            var skinnedMeshRenderers = go.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers)
            {
                mesh = skinnedMeshRenderer.sharedMesh;
                Vector3 position = skinnedMeshRenderer.transform.position + (Vector3)localToWorld.ValueRO.Position;
                Quaternion rotation = skinnedMeshRenderer.transform.rotation * localToWorld.ValueRO.Rotation;
                Vector3 scale = skinnedMeshRenderer.transform.localScale;
                mats = skinnedMeshRenderer.sharedMaterials;
                list.Add(new MeshPreview
                {
                    Mesh = mesh,
                    Materials = mats,
                    Position = position,
                    Rotation = rotation,
                    Scale = scale
                });
            }


            foreach (MeshPreview meshPreview in list)
                DrawMesh(meshPreview.Position, meshPreview.Rotation, meshPreview.Scale, meshPreview.Mesh,
                    meshPreview.Materials);
        }
    }

    public class MeshPreview
    {
        public Material[] Materials;
        public Mesh Mesh;
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;
    }

    public static void DrawMesh(Vector3 position, Quaternion rotation, Vector3 scale, Mesh mesh, Material[] materials)
    {
        Matrix4x4 matrix = Matrix4x4.TRS(position,
            rotation, scale);


        for (var sub = 0; sub < mesh.subMeshCount; sub++)
        {
            // safeguard if materials array is shorter than sub-mesh count
            Material mat = sub < materials.Length
                ? materials[sub]
                : materials[0];

            // layer: use your GameObject's layer (or a fixed one), and pass your camera (or null)
            Graphics.DrawMesh(
                mesh,
                matrix,
                mat,
                0, // layer
                Camera.main, // camera (or null for all)
                sub // sub-mesh index
            );
        }
    }
}