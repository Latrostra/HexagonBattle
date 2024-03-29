﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class HexGrid : MonoBehaviour
{
    [SerializeField] private int _width = 10;
    [SerializeField] private int _height = 10;

    [SerializeField] private Text _cellLabelPrefab;
    [SerializeField] private HexCell _cellPrefab;

    [SerializeField] private Color defaultColor = Color.white;

    private HexCell[] cells;
    private Canvas gridCanvas;
    private HexMesh hexMesh;

    private void Awake()
    {
        gridCanvas = GetComponentInChildren<Canvas>();
        hexMesh = GetComponentInChildren<HexMesh>();
        cells = new HexCell[_width * _height];

        for(int z = 0, i = 0; z < _height; z++) {
            for(int x = 0; x < _width; x++) {
                CreateCell(x, z, i++);
            }
        }
    }
    private void Start()
    {
        hexMesh.Triangulate(cells);
    }
    public HexCell GetCell(Vector3 position)
    {
        position = transform.InverseTransformPoint(position);
        HexCoordinates coordinates = HexCoordinates.FromPostion(position);
        int index = coordinates.X + coordinates.Z * _width + coordinates.Z / 2;
        Debug.Log("touched at " + coordinates.ToString());
        return cells[index];
    }
    public void Refresh() {
        hexMesh.Triangulate(cells);
    }
    private void CreateCell(int x, int z, int i)
    {
        Vector3 position = new Vector3((x + z * 0.5f - z / 2) * (HexMetric.innerRadius * 2f), 0f, z * HexMetric.outerRadius * 1.5f);
        HexCell cell = Instantiate<HexCell>(_cellPrefab);
        cells[i] = cell;
        cell.transform.SetParent(transform, false);
        cell.transform.localPosition = position;
        cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
        cell.color = defaultColor;
        
        if (x > 0) {
            cell.SetNeighbour(HexDirection.W, cells[i - 1]);
        }
        if (z > 0) {
            if ((z & 1) == 0) {
                cell.SetNeighbour(HexDirection.SE, cells[i - _width]);
                if (x > 0) {
                    cell.SetNeighbour(HexDirection.SW, cells[i - _width - 1]);
                }
            }
            else {
                cell.SetNeighbour(HexDirection.SW, cells[i - _width]);
                if (x < _width - 1) {
                    cell.SetNeighbour(HexDirection.SE, cells[i - _width + 1]);
                }
            }
        }

        Text label = Instantiate<Text>(_cellLabelPrefab);
        label.rectTransform.SetParent(gridCanvas.transform, false);
        label.rectTransform.anchoredPosition = new Vector2(position.x, position.z);
        label.text = cell.coordinates.ToStringOnSeparateLines();

        cell.uiRect = label.rectTransform;
    }
}
