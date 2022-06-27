using System;
using UnityEngine;
using UnityEngine.UI;

public class HexGrid : MonoBehaviour
{
    [SerializeField]private Text cellLabelPrefab;
	[SerializeField]private HexCell cellPrefab;
	[SerializeField]public Material touchedMaterial;
	[SerializeField]public Material defaultMaterial;
	[SerializeField]public Texture2D noiseSource;
	[SerializeField] public HexMapEditor editor;
	[SerializeField]public int chunkCountX = 4, chunkCountZ = 3;
	private int cellCountX, cellCountZ;
	HexGridChunk[] chunks;
	public HexGridChunk chunkPrefab;
    private HexCell[] cells;

	void Awake () {
		HexMetrics.noiseSource = noiseSource;
		cellCountX = chunkCountX * HexMetrics.chunkSizeX;
		cellCountZ = chunkCountZ * HexMetrics.chunkSizeZ;
		
		CreateChunks();
		CreateCells();
		
	}

    private void CreateChunks()
    {
		chunks = new HexGridChunk[chunkCountX * chunkCountZ];

		for (int z = 0, i = 0; z < chunkCountZ; z++) {
			for (int x = 0; x < chunkCountX; x++) {
				HexGridChunk chunk = chunks[i++] = Instantiate(chunkPrefab);
				chunk.transform.SetParent(transform);
			}
		}
    }

    private void CreateCells(){
		cells = new HexCell[cellCountZ  * cellCountX];
		for (int z = 0, i = 0; z < cellCountZ; z++) {
			for (int x = 0; x < cellCountX; x++) {
				CreateCell(x, z, i++);
			}
		}
	}
	void OnEnable () {
		HexMetrics.noiseSource = noiseSource;
	}
	
	private void Update() {
        if (Input.GetMouseButton(0)) {
			HandleInput();
		}
    }
    void HandleInput(){
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if(Physics.Raycast(inputRay,out hit)){
			if(editor != null){
				editor.EditCell(GetCell(hit.point),this);
			}else{

           		GetCell(hit.point);
			}
        }
    }
	
    public HexCell GetCell(Vector3 position)
    {
		position = transform.InverseTransformPoint(position);
		HexCoordinates coordinates = HexCoordinates.FromPosition(position);
		int index = coordinates.X + coordinates.Z * cellCountX + coordinates.Z /2;
		return cells[index];
    }

    void CreateCell (int x, int z, int i) {
		Vector3 position;
		position.x =(x+z*0.5f -z/2) * (HexMetrics.innerRadius * 2f);
		position.y = 0f;
		position.z = z * (HexMetrics.outerRadius * 1.5f);

		HexCell cell = cells[i] = Instantiate<HexCell>(cellPrefab);
		cell.transform.localPosition = position;
		cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
		cell.material = defaultMaterial;

		if(x>0){
			cell.SetNeighbor(HexDirection.W, cells[i-1]);
		}
		if(z>0){
			if((z & 1)==0){
				cell.SetNeighbor(HexDirection.SE, cells[i-cellCountX]);
				if(x>0){
					cell.SetNeighbor(HexDirection.SW,cells[i-cellCountX-1]);
				}
			}else{
				cell.SetNeighbor(HexDirection.SW, cells[i-cellCountX]);
				if(x < cellCountX-1){
					cell.SetNeighbor(HexDirection.SE,cells[i-cellCountX+1]);
				}
			}
		}

		Text label = Instantiate<Text>(cellLabelPrefab);
		label.rectTransform.anchoredPosition =
			new Vector2(position.x, position.z);
		label.text = cell.coordinates.ToStringOnSeparateLines();
		cell.uiRect = label.rectTransform;
		
		cell.Elevation = 0;

		AddCellToChunk(x, z, cell);
	}

    private void AddCellToChunk(int x, int z, HexCell cell)
    {
        int chunkX = x / HexMetrics.chunkSizeX;
		int chunkZ = z / HexMetrics.chunkSizeZ;
		HexGridChunk chunk = chunks[chunkX + chunkZ * chunkCountX];

		int localX = x - chunkX * HexMetrics.chunkSizeX;
		int localZ = z - chunkZ * HexMetrics.chunkSizeZ;
		chunk.AddCell(localX + localZ * HexMetrics.chunkSizeX, cell);
    }
}
