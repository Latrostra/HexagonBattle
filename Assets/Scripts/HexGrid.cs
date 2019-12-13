using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class HexGrid : MonoBehaviour
{
    [SerializeField] private int _width = 10;
    [SerializeField] private int _height = 10;

    [SerializeField] private Text _cellLabelPrefab;
    [SerializeField] private HexCell _cellPrefab;

    private HexCell[] cells;

    Canvas gridCanvas;

    private void Awake()
    {
        gridCanvas = GetComponentInChildren<Canvas>();
        cells = new HexCell[_width * _height];

        for(int z = 0, i = 0; z < _height; z++) {
            for(int x = 0; x < _width; x++) {
                CreateCell(x, z, i++);
            }
        }
    }

    private void CreateCell(int x, int z, int i)
    {
        Vector3 position = new Vector3(x * 10f, 0f, z * 10f);

        HexCell cell = Instantiate<HexCell>(_cellPrefab);
        cells[i] = cell;
        cell.transform.SetParent(transform, false);
        cell.transform.localPosition = position;

        Text label = Instantiate<Text>(_cellLabelPrefab);
        label.rectTransform.SetParent(gridCanvas.transform, false);
        label.rectTransform.anchoredPosition = new Vector2(position.x, position.z);
        label.text = x.ToString() + "\n" + z.ToString(); 
    }
}
