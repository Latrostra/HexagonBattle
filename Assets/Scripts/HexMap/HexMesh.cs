﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexMesh : MonoBehaviour
{
    private Mesh hexMesh;
    private List<Vector3> vertices;
    private List<int> triangles;
    private MeshCollider meshCollider;
    private List<Color> colors;
    private void Awake()
    {
        meshCollider = gameObject.AddComponent<MeshCollider>();
        GetComponent<MeshFilter>().mesh = hexMesh = new Mesh();
        hexMesh.name = "Hex Mesh";
        vertices = new List<Vector3>();
        colors = new List<Color>();
        triangles = new List<int>();
    }

    public void Triangulate(HexCell[] cells)
    {
        hexMesh.Clear();
        vertices.Clear();
        colors.Clear();
        triangles.Clear();
        for(int i = 0; i < cells.Length; i++) {
            Triangulate(cells[i]);
        }
        hexMesh.vertices = vertices.ToArray();
        hexMesh.colors = colors.ToArray();
        hexMesh.triangles = triangles.ToArray();
        hexMesh.RecalculateNormals();
        meshCollider.sharedMesh = hexMesh;
    }
    private void Triangulate(HexCell cell) {
        for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++) {
            Triangulate(d, cell);
        }
    }
    private void Triangulate(HexDirection direction, HexCell cell) {
        Vector3 center = cell.transform.localPosition;
        Vector3 v1 = center + HexMetric.GetFirstSolidCorner(direction);
        Vector3 v2 = center + HexMetric.GetSecondSolidCorner(direction);

        AddTriangle(center, v1, v2);
        AddTriangleColor(cell.color);
        if (direction <= HexDirection.SE) {
            TriangulateConnection(direction, cell, v1, v2);
        }
    }
    private void TriangulateConnection(HexDirection direction, HexCell cell, Vector3 v1, Vector3 v2) {

        HexCell neighbour = cell.GetNeighbour(direction);
        if (neighbour == null) {
            return;
        }
        Vector3 bridge = HexMetric.GetBridge(direction);
        Vector3 v3 = v1 + bridge;
        Vector3 v4 = v2 + bridge;
        v3.y = v4.y = neighbour.Elevation * HexMetric.elevationStep;
        if (cell.GetEdgeType(direction) == HexEdgeType.Slope) {
            TriangulateEdgeTerraces(v1, v2, cell, v3, v4, neighbour);
        } 
        else {
            AddQuad(v1, v2, v3, v4);
			AddQuadColor(cell.color, neighbour.color);
        }

        HexCell nextNeighbour = cell.GetNeighbour(direction.Next());
        if (direction <= HexDirection.E && nextNeighbour != null) {
            Vector3 v5 = v2 + HexMetric.GetBridge(direction.Next());
            v5.y = nextNeighbour.Elevation * HexMetric.elevationStep;

            if (cell.Elevation <= neighbour.Elevation) {
                if (cell.Elevation <= nextNeighbour.Elevation) {
                    TriangulateCorner(v2, cell, v4, neighbour, v5, nextNeighbour);
                }
                else {
                    TriangulateCorner(v5, nextNeighbour, v2, cell, v4, neighbour);
                }
            }
            else if (neighbour.Elevation <= nextNeighbour.Elevation) {
                TriangulateCorner(v4, neighbour, v5, nextNeighbour, v2, cell);
            }
            else {
                TriangulateCorner(v5, nextNeighbour, v2, cell, v4, neighbour);
            }
        }
    }
    private void TriangulateEdgeTerraces (
		Vector3 beginLeft, Vector3 beginRight, HexCell beginCell,
		Vector3 endLeft, Vector3 endRight, HexCell endCell
	) {
		Vector3 v3 = HexMetric.TerraceLerp(beginLeft, endLeft, 1);
		Vector3 v4 = HexMetric.TerraceLerp(beginRight, endRight, 1);
		Color c2 = HexMetric.TerraceLerp(beginCell.color, endCell.color, 1);

		AddQuad(beginLeft, beginRight, v3, v4);
		AddQuadColor(beginCell.color, c2);

		for (int i = 2; i < HexMetric.terraceSteps; i++) {
			Vector3 v1 = v3;
			Vector3 v2 = v4;
			Color c1 = c2;
			v3 = HexMetric.TerraceLerp(beginLeft, endLeft, i);
			v4 = HexMetric.TerraceLerp(beginRight, endRight, i);
			c2 = HexMetric.TerraceLerp(beginCell.color, endCell.color, i);
			AddQuad(v1, v2, v3, v4);
			AddQuadColor(c1, c2);
		}

		AddQuad(v3, v4, endLeft, endRight);
		AddQuadColor(c2, endCell.color);
	}
    private void TriangulateCorner(
        Vector3 bottom, HexCell bottomCell,
        Vector3 left, HexCell leftCell,
        Vector3 right, HexCell rightCell
    ) {
        HexEdgeType leftEdgeType = bottomCell.GetEdgeType(leftCell);
        HexEdgeType rightEdgeType = bottomCell.GetEdgeType(rightCell);
        
        if (leftEdgeType == HexEdgeType.Slope) {
			if (rightEdgeType == HexEdgeType.Slope) {
				TriangulateCornerTerraces(
					bottom, bottomCell, left, leftCell, right, rightCell
				);
			}
			else if (rightEdgeType == HexEdgeType.Flat) {
				TriangulateCornerTerraces(
					left, leftCell, right, rightCell, bottom, bottomCell
				);
			}
			else {
				TriangulateCornerTerracesCliff(
					bottom, bottomCell, left, leftCell, right, rightCell
				);
			}
		}
		else if (rightEdgeType == HexEdgeType.Slope) {
			if (leftEdgeType == HexEdgeType.Flat) {
				TriangulateCornerTerraces(
					right, rightCell, bottom, bottomCell, left, leftCell
				);
			}
			else {
				TriangulateCornerCliffTerraces(
					bottom, bottomCell, left, leftCell, right, rightCell
				);
			}
		}
		else if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope) {
			if (leftCell.Elevation < rightCell.Elevation) {
				TriangulateCornerCliffTerraces(
					right, rightCell, bottom, bottomCell, left, leftCell
				);
			}
			else {
				TriangulateCornerTerracesCliff(
					left, leftCell, right, rightCell, bottom, bottomCell
				);
			}
		}
		else {
			AddTriangle(bottom, left, right);
			AddTriangleColor(bottomCell.color, leftCell.color, rightCell.color);
        }
        AddTriangle(bottom, left, right);
		AddTriangleColor(bottomCell.color, leftCell.color, rightCell.color);
    }
    private void TriangulateCornerTerracesCliff (
        Vector3 begin, HexCell beginCell,
        Vector3 left, HexCell leftCell,
        Vector3 right, HexCell rightCell
    ) {
        float b = 1f / (rightCell.Elevation - beginCell.Elevation);
        if (b < 0) {
            b = -b;
        }
        Vector3 boundary = Vector3.Lerp(begin, right, b);
        Color boundaryColor = Color.Lerp(beginCell.color, rightCell.color, b);
        
        TriangulateBoundaryTriangle(
            begin, beginCell, left, leftCell, boundary, boundaryColor
            );
        if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope) {
            TriangulateBoundaryTriangle(
                left, leftCell, right, rightCell, boundary, boundaryColor
            );
        }
        else {
            AddTriangle(left, right, boundary);
            AddTriangleColor(leftCell.color, rightCell.color, boundaryColor);
        }
    }
    void TriangulateCornerCliffTerraces (
		Vector3 begin, HexCell beginCell,
		Vector3 left, HexCell leftCell,
		Vector3 right, HexCell rightCell
	) {
		float b = 1f / (leftCell.Elevation - beginCell.Elevation);
        if (b < 0) {
            b = -b;
        }
		Vector3 boundary = Vector3.Lerp(begin, left, b);
		Color boundaryColor = Color.Lerp(beginCell.color, leftCell.color, b);
        

		TriangulateBoundaryTriangle(
			right, rightCell, begin, beginCell, boundary, boundaryColor
		);

		if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope) {
			TriangulateBoundaryTriangle(
				left, leftCell, right, rightCell, boundary, boundaryColor
			);
		}
		else {
			AddTriangle(left, right, boundary);
			AddTriangleColor(leftCell.color, rightCell.color, boundaryColor);
		}
	}
    private void TriangulateBoundaryTriangle(
        Vector3 begin, HexCell beginCell, 
        Vector3 left, HexCell leftCell, 
        Vector3 boundary, Color boundaryColor
        )
    {
        Vector3 v2 = HexMetric.TerraceLerp(begin, left, 1);
        Color c2 = HexMetric.TerraceLerp(beginCell.color, leftCell.color, 1);

        AddTriangle(begin, v2, boundary);
        AddTriangleColor(beginCell.color, c2, boundaryColor);
        for (int i = 2; i < HexMetric.terraceSteps; i++) {
            Vector3 v1 = v2;
            Color c1 = c2;
            v2 = HexMetric.TerraceLerp(begin, left, i);
            c2 = HexMetric.TerraceLerp(beginCell.color, leftCell.color, i);
            AddTriangle(v1, v2, boundary);
            AddTriangleColor(c1, c2, boundaryColor);
        }
        AddTriangle(v2, left, boundary);
        AddTriangleColor(c2, leftCell.color, boundaryColor);
    }

    private void TriangulateCornerTerraces(
        Vector3 begin, HexCell beginCell,
        Vector3 left, HexCell leftCell,
        Vector3 right, HexCell rightCell
    ) {
        Vector3 v3 = HexMetric.TerraceLerp(begin, left, 1);
        Vector4 v4 = HexMetric.TerraceLerp(begin, right, 1);
        Color c3 = HexMetric.TerraceLerp(beginCell.color, leftCell.color, 1);
        Color c4 = HexMetric.TerraceLerp(beginCell.color, rightCell.color, 1);

        AddTriangle(begin, v3, v4);
        AddTriangleColor(beginCell.color, c3, c4);

        for (int i = 2; i < HexMetric.terraceSteps; i++) {
			Vector3 v1 = v3;
			Vector3 v2 = v4;
			Color c1 = c3;
			Color c2 = c4;
			v3 = HexMetric.TerraceLerp(begin, left, i);
			v4 = HexMetric.TerraceLerp(begin, right, i);
			c3 = HexMetric.TerraceLerp(beginCell.color, leftCell.color, i);
			c4 = HexMetric.TerraceLerp(beginCell.color, rightCell.color, i);
			AddQuad(v1, v2, v3, v4);
			AddQuadColor(c1, c2, c3, c4);
		}

		AddQuad(v3, v4, left, right);
		AddQuadColor(c3, c4, leftCell.color, rightCell.color);
    }
    private void AddTriangleColor(Color c1)
    {
        colors.Add(c1);
        colors.Add(c1);
        colors.Add(c1);
    }

    private void AddTriangleColor(Color c1, Color c2, Color c3)
    {
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c3);
    }
    private void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        int vertexIndex = vertices.Count;
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
    }
    private void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4) {
        int vertexIndex = vertices.Count;
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);
        vertices.Add(v4);
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 3);
    }
    private void AddQuadColor(Color c1, Color c2) {
        colors.Add(c1);
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c2);
    }
    private void AddQuadColor(Color c1, Color c2, Color c3, Color c4) {
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c3);
        colors.Add(c4);
    }
}
