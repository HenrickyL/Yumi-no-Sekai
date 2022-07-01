using UnityEngine;
using UnityEngine.UI;
using System.IO;
public class HexGrid : MonoBehaviour
{
    [SerializeField]private Text cellLabelPrefab;
	[SerializeField]private HexCell cellPrefab;
	[SerializeField]public Texture2D noiseSource;
	public int cellCountX = 20, cellCountZ = 15;
	int chunkCountX, chunkCountZ;
	HexGridChunk[] chunks;
	public HexGridChunk chunkPrefab;
    private HexCell[] cells;
	public Color[] colors;
	void Awake () {
		HexMetrics.noiseSource = noiseSource;
		HexMetrics.colors = colors;
		
		CreateMap(cellCountX, cellCountZ);
	}

	public void CreateMap (int x, int z) {
		ValidateMapSizes(x,z);
		ClearOldData();
		cellCountX = x;
		cellCountZ = z;
		chunkCountX = cellCountX / HexMetrics.chunkSizeX;
		chunkCountZ = cellCountZ / HexMetrics.chunkSizeZ;
		CreateChunks();
		CreateCells();
	}

	private void ClearOldData(){
		if (chunks != null) {
			for (int i = 0; i < chunks.Length; i++) {
				Destroy(chunks[i].gameObject);
			}
		}
	}
	private void ValidateMapSizes(int x, int z){
		if (
			x <= 0 || x % HexMetrics.chunkSizeX != 0 ||
			z <= 0 || z % HexMetrics.chunkSizeZ != 0
		) {
			Debug.LogError("Unsupported map size.");
			return;
		}
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
		if(!HexMetrics.noiseSource){
			HexMetrics.noiseSource = noiseSource;
			HexMetrics.colors = colors;
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
	public HexCell GetCell (HexCoordinates coordinates) {
		int z = coordinates.Z;
		if (z < 0 || z >= cellCountZ) {
			return null;
		}
		int x = coordinates.X + z / 2;
		if (x < 0 || x >= cellCountX) {
			return null;
		}
		return cells[x + z * cellCountX];
	}
	public void ShowUI (bool visible) {
		for (int i = 0; i < chunks.Length; i++) {
			chunks[i].ShowUI(visible);
		}
	}

	public void Save (BinaryWriter writer) {
		writer.Write(cellCountX);
		writer.Write(cellCountZ);
		for (int i = 0; i < cells.Length; i++) {
			cells[i].Save(writer);
		}
	}

	public void Load (BinaryReader reader,  int header) {
		//old version
		int x = 20, z = 15;
		if (header >= 1) {
			x = reader.ReadInt32();
			z = reader.ReadInt32();
		}
		CreateMap(reader.ReadInt32(), reader.ReadInt32());
		for (int i = 0; i < cells.Length; i++) {
			cells[i].Load(reader);
		}
		for (int i = 0; i < chunks.Length; i++) {
			chunks[i].Refresh();
		}
	}
	
}
