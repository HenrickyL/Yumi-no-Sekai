using UnityEngine;
using UnityEngine.UI;

public class HexGrid : MonoBehaviour
{
    [SerializeField]private Text cellLabelPrefab;
    [SerializeField]private int width = 6;
	[SerializeField]private int height = 6;
	[SerializeField]private HexCell cellPrefab;
	[SerializeField]public Material touchedMaterial;
	[SerializeField]public Material defaultMaterial;
	[SerializeField]public Texture2D noiseSource;
	[SerializeField] public HexMapEditor editor;

    private HexCell[] cells;
	private Canvas gridCanvas;
	public HexMesh hexMesh;

	void Awake () {
		HexMetrics.noiseSource = noiseSource;
        gridCanvas = GetComponentInChildren<Canvas>();
        hexMesh = GetComponentInChildren<HexMesh>();

		cells = new HexCell[height * width];
		for (int z = 0, i = 0; z < height; z++) {
			for (int x = 0; x < width; x++) {
				CreateCell(x, z, i++);
			}
		}
	}
	void OnEnable () {
		HexMetrics.noiseSource = noiseSource;
	}
	private void Start() {
		hexMesh.Triangulate(cells);
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
		int index = coordinates.X + coordinates.Z * width + coordinates.Z /2;
		return cells[index];

		// HexCell cell = cells[index];
		// cell.selected = !cell.selected;
		// if(cell.selected){
		// 	cell.material = colorMaterial;
		// }else{
		// 	cell.material = defaultMaterial;
		// }
		// hexMesh.Triangulate(cells);
		// Debug.Log("touch at "+coordinates.ToString());
    }
	public void Refresh(){
		hexMesh.Triangulate(cells);
	}

    void CreateCell (int x, int z, int i) {
		Vector3 position;
		position.x =(x+z*0.5f -z/2) * (HexMetrics.innerRadius * 2f);
		position.y = 0f;
		position.z = z * (HexMetrics.outerRadius * 1.5f);

		HexCell cell = cells[i] = Instantiate<HexCell>(cellPrefab);
		cell.transform.SetParent(transform, false);
		cell.transform.localPosition = position;
		cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
		// cell.color = defaultColor;
		cell.material = defaultMaterial;

		if(x>0){
			cell.SetNeighbor(HexDirection.W, cells[i-1]);
		}
		if(z>0){
			if((z & 1)==0){
				cell.SetNeighbor(HexDirection.SE, cells[i-width]);
				if(x>0){
					cell.SetNeighbor(HexDirection.SW,cells[i-width-1]);
				}
			}else{
				cell.SetNeighbor(HexDirection.SW, cells[i-width]);
				if(x < width-1){
					cell.SetNeighbor(HexDirection.SE,cells[i-width+1]);
				}
			}
		}

		Text label = Instantiate<Text>(cellLabelPrefab);
		label.rectTransform.SetParent(gridCanvas.transform, false);
		label.rectTransform.anchoredPosition =
			new Vector2(position.x, position.z);
		label.text = cell.coordinates.ToStringOnSeparateLines();
		cell.uiRect = label.rectTransform;
		
		cell.Elevation = 0;
	}

	
}
