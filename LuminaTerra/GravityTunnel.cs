using System;
using System.Collections.Generic;
using System.Linq;
using DitzyExtensions;
using DitzyExtensions.Collection;
using LuminaTerra.Util;
using OWML.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace LuminaTerra;

[ExecuteInEditMode]
public class GravityTunnel : GeneratingMonoBehavior
{
    private static float Multiplier = 100f;
    
    [SerializeField] private bool liveEdit = false;
    [SerializeField] private bool preserveOnDisable = false;
    [SerializeField] private float groundPointMinimumSeparation = 0.01f;
    [SerializeField] private float volumeHeight = 1f;
    [SerializeField] private float volumeWidth = 1f;
    [SerializeField] private float volumeHeightBias = 1f;
    [SerializeField] private float volumeLengthOverlap = 0f;
    [SerializeField] private float gravityMagnitude = 1f;
    
    private readonly IList<Vertex> _groundPoints = [];

    protected override void OnEnable()
    {
        base.OnEnable();
        var mesh = GetComponent<MeshFilter>()?.sharedMesh;
        if (!mesh) return;

        _groundPoints.Clear();
        var verts = ConsolidatePoints(mesh);
        for (var i = 0; i < verts.Count - 1; i++)
        {
            var start = verts[i];
            var end = verts[i + 1];
            _groundPoints.Add(new Vertex(
                (start.Pos + end.Pos)/2,
                ((start.Up + end.Up)/2).normalized
            )
            {
                Length = Multiplier * Vector3.Distance(start.Pos, end.Pos),
                Forward = (end.Pos - start.Pos).normalized
            });
        }

        CreateForceVolumes();
        UpdateForceVolumes();
    }

    protected override void OnDisable()
    {
        if (preserveOnDisable) return;
        
        _groundPoints.Clear();
        base.OnDisable();
    }

    private void Update()
    {
        if (!IsInEditor) return;
        if (!liveEdit) return;

        UpdateForceVolumes();
    }

    private void OnDrawGizmosSelected()
    {
        Matrix4x4 matrix = Gizmos.matrix;
        Gizmos.matrix = matrix * Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
        
        _groundPoints.ForEach(v =>
        {
            Gizmos.DrawLine(v.Pos, v.Pos + v.Up/10);
            Gizmos.DrawLine(v.Pos, v.Pos + v.Forward/10);
        });
        
        Gizmos.matrix = matrix;
    }

    private IList<Vertex> ConsolidatePoints(Mesh mesh)
    {
        var consolidatedVerts = new List<Vertex>();
        var indexMap = new Dictionary<int, int>();
        var cvi = 0;
        var seenVerts = new bool[mesh.vertexCount];
        mesh.vertices.ForEach((vert, i) =>
        {
            if (seenVerts[i]) return;
            seenVerts[i] = true;
            
            indexMap.Add(i, cvi);

            var consolidatedVert = vert;
            var consolidatedNorm = mesh.normals[i];
            var groupCount = 1;
            for (var j = i + 1; j < mesh.vertexCount; j++)
            {
                if (seenVerts[j]) continue;

                if (groundPointMinimumSeparation < Vector3.Distance(vert, mesh.vertices[j])) continue;
                seenVerts[j] = true;

                indexMap.Add(j, cvi);
                groupCount++;
                consolidatedVert += mesh.vertices[j];
                consolidatedNorm += mesh.normals[j];
            }
            
            consolidatedVerts.Add(new Vertex(
                consolidatedVert / groupCount,
                (consolidatedNorm / groupCount).normalized
            ));
            cvi++;
        });
        // Debug.Log(indexMap.AsString());

        var edges = new Dictionary<int, List<int>>();
        var seenEdges = new HashSet<int>();

        void AddEdge(int a, int b)
        {
            var hash = a * consolidatedVerts.Count + b;
            if (!seenEdges.Add(hash)) return;
            seenEdges.Add(b * consolidatedVerts.Count + a);

            edges.TryAdd(a, []);
            edges[a].Add(b);
            edges.TryAdd(b, []);
            edges[b].Add(a);
        }

        for (var i = 0; i < mesh.triangles.Length - 3; i+=3)
        {
            var a = mesh.triangles[i];
            var b = mesh.triangles[i+1];
            var c = mesh.triangles[i+2];
            var cva = indexMap[a];
            var cvb = indexMap[b];
            var cvc = indexMap[c];
            if (cva != cvb) AddEdge(cva, cvb);
            if (cvb != cvc) AddEdge(cvb, cvc);
            if (cvc != cva) AddEdge(cvc, cva);
        }
        
        // Debug.Log(edges.Select(kv => (kv.Key, kv.Value.AsString())).AsDict().AsString());

        var vertIx = edges
            .SelectWhere(edge => (edge.Value.Count == 1, edge.Key))
            .First();

        IList<Vertex> orderedVerts = [consolidatedVerts[vertIx]];
        foreach (var _ in consolidatedVerts)
        {
            if (edges[vertIx].IsEmpty()) break;

            var bIx = edges[vertIx][0];
            edges[vertIx].Remove(bIx);
            edges[bIx].Remove(vertIx);
            vertIx = bIx;
            orderedVerts.Add(consolidatedVerts[vertIx]);
        };

        return orderedVerts;
    }

    private void CreateForceVolumes()
    {
        _groundPoints.ForEach(point =>
        {
            var volume = GenerateObject();
            point.Volume = volume;
            volume.transform.localPosition = point.Pos;
            volume.transform.localRotation = Quaternion.LookRotation(
                Vector3.ProjectOnPlane(point.Forward, point.Up),
                point.Up
            );
            volume.transform.localScale = Vector3.one / Multiplier;
            volume.GetAddComponent<OWTriggerVolume>();
            var dfv = volume.GetAddComponent<DirectionalForceVolume>();
            point.Shape = volume.GetAddComponent<BoxShape>();
            
            dfv.SetLocalForceDirection(Vector3.down);
            dfv.SetFieldMagnitude(gravityMagnitude);
        });
    }

    private void UpdateForceVolumes()
    {
        _groundPoints.ForEach(vert =>
        {
            vert.Shape.center = vert.Pos + volumeHeightBias*Vector3.up;
            vert.Shape.size = new Vector3(
                volumeWidth,
                volumeHeight,
                vert.Length + volumeLengthOverlap
            );
        });
    }
}

public class Vertex(
    Vector3 pos,
    Vector3 up
)
{
    public Vector3 Pos { get; set; } = pos;
    public Vector3 Up { get; set; } = up;
    public Vector3 Forward { get; set; } = Vector3.zero;
    public float Length { get; set; } = 0f;
    public GameObject Volume { get; set; } = null!;
    public BoxShape Shape { get; set; } = null!;
};