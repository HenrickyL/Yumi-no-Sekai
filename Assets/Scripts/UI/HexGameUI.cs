using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HexGameUI : MonoBehaviour {

	public HexGrid grid;
	float waitTime = 1f;
	HexCell currentCell, hoverCell, previosCell, destinationCell;
	public Text TurnsText;
	public UnitInfo selectInfo;
	private Color colorHover = new Color(0,0,0,0.15f),
		 colorSelectedUnity = new Color(0,0,1,0.3f),
		 colorMove = new  Color(0.98f,0.83f,0.29f,0.5f),
		 colorMoveHovered = Color.white,
		 colorEnemy = new Color(1,0.65f,0,0.5f),
		 colorEnemyHovered = new Color(1,0.65f,0),
		 colorAttack = new Color(1,0,0,0.5f),
		 colorAttackHovered = Color.red,
		 colorSelected = new Color(0,0,1,0.5f),
		 colorSelectedHevered = Color.blue;
	Color colorActiveAction, colorActiveActionHovered;
	private List<Color> actionsColors, actionColorHovered;
	public bool IsAttackMode { get{return unitActionType == UnitActionsEnum.Attack;} }
	public bool IsMoveMode { get{return unitActionType == UnitActionsEnum.Move;}}
	public bool IsWaitMode { get{return unitActionType == UnitActionsEnum.Wait;} }

	HexUnit selectedUnit, previosSelected;
	bool inAction = false;
	UnitActionsEnum unitActionType= UnitActionsEnum.Wait;
	UnitActionsEnum subUniActionType= UnitActionsEnum.Wait;

	public int Turns { get; set; }
	
	public void SetEditMode (bool toggle) {
		enabled = !toggle;
		grid.ShowUI(!toggle);
		grid.ClearPath();
	}


	private void OnEnable() {
		Turns = 0;
		actionsColors = new List<Color>(){
			colorMove,colorAttack, colorSelected
		};
		actionColorHovered = new List<Color>(){
			colorMoveHovered, colorAttackHovered, colorSelectedHevered
		};
		SetWaitMode();
	}
	void Update () {
		if (!EventSystem.current.IsPointerOverGameObject()) {
			HoveredCell();
			UpdateUI();
			UpdateCurrentCell();
			if (Input.GetMouseButtonUp(0)) {
				if(!selectedUnit || IsWaitMode){
					DoSelection();
				}else{
					if(IsAttackMode){
						DoNormalAttack(currentCell);
					}else if(IsMoveMode){
						DoMove(currentCell);
					}					
				}
			}
			else if(Input.GetKeyUp(KeyCode.Space)){
				foreach(var u in grid.Units){
						u.AutomaticAggressiveMovement();	
				}
			}
			else if(Input.GetKeyUp(KeyCode.LeftShift)){
				DoPathfinding();
			}
		}
	}

    

    void SetHighlightHover(HexCell prev, HexCell next, Color? prevColor, Color nextColor){
		if(prevColor== null || prevColor ==  Color.clear){
			prev.DisableHighlight();
		}else{
			prev.EnableHighlight(prevColor ?? Color.red);
		}
		hoverCell = next;
		hoverCell.EnableHighlight(nextColor);
	}
	void HoveredCell(){
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		HexCell previous = hoverCell;
		if(!previous){
			hoverCell = grid.GetCell(ray);
			return;
		}
		HexCell next = grid.GetCell(ray);
		if( hoverCell && previous && next && next != previous ){
			HighlightSelectUnitAction();
			if(selectedUnit && !IsWaitMode){
				bool prevInSelect = IsAttackMode?
					selectedUnit.IsValidAttackDestination(previous):
					selectedUnit.IsValidMoveDestination(previous);
				bool nextInSelect = IsAttackMode?
					selectedUnit.IsValidAttackDestination(next):
					selectedUnit.IsValidMoveDestination(next);
				
				if(prevInSelect && next == selectedUnit.Location){
					SetHighlightHover(previous,next,colorActiveAction, colorSelectedHevered);
				}
				else if(prevInSelect && nextInSelect){
					SetHighlightHover(previous,next,colorActiveAction, colorActiveActionHovered);
				}
				else if(!prevInSelect && nextInSelect){
					SetHighlightHover(previous,next,null, colorActiveActionHovered);
				}
				else if(prevInSelect && !nextInSelect){
					SetHighlightHover(previous,next,colorActiveAction, colorHover);
				}
				else if(previous == selectedUnit.Location && nextInSelect){
					SetHighlightHover(previous,next,
						IsAttackMode? colorSelected:colorSelectedUnity,
						IsAttackMode && next.Unit? colorAttackHovered: colorMoveHovered);
				}
				else{
					SetHighlightHover(previous,next,null, currentCell.Unit && currentCell.Unit.type == UnitType.Enemy? colorEnemy:colorHover);
				}
			}
			else if(selectedUnit){
				if(next == selectedUnit.Location){
					SetHighlightHover(previous,next,null, colorSelectedHevered);
				}else{
					SetHighlightHover(previous,next,null, currentCell.Unit && currentCell.Unit.type == UnitType.Enemy? colorEnemy:colorHover);
				}
			}else{
					SetHighlightHover(previous,next,null, currentCell.Unit && currentCell.Unit.type == UnitType.Enemy? colorEnemy:colorHover);
			}
			
		}
	}
	

	void DoSelection () {
		UpdateCurrentCell();
		if(currentCell ){
			if(currentCell.Unit != selectedUnit){
				SwapSelectedUnit(currentCell.Unit);
			}
		}
		SetWaitMode();
	}
	void HighlightSelectUnitAction(){
		previosSelected?.ClearHighlights();
		if(selectedUnit){
			if(unitActionType == UnitActionsEnum.Move){
				selectedUnit.EnableHighlightMove(colorMove);
			}else if(unitActionType == UnitActionsEnum.Attack){
				selectedUnit.EnableHighlightAttack(colorAttack);
			}else{
				selectedUnit.EnableHighlight(colorSelected);
			}
		}
	}
	
	

	void DoPathfinding (HexCell destination) {
		inAction = true;
		grid.ClearPath();
		if (currentCell && selectedUnit &&
			selectedUnit.IsValidFullDestination(destination) &&
			selectedUnit.IsValidMoveDestination(destination)) 
		{
			grid.FindPath(selectedUnit.Location, destination, 5*selectedUnit.Speed+1);
		}
		else {
			grid.ClearPath();
		}
	}
	void DoPathfinding () {
		grid.ClearPath();
		if (currentCell) 
		{
			grid.FindPath(selectedUnit.Location, currentCell, 5*selectedUnit.Speed+1);
		}
		else {
			grid.ClearPath();
		}
	}

	void DoMove (HexCell cell) {
		if (cell && selectedUnit) {
			previosCell = currentCell;
			destinationCell = cell;
			selectedUnit.MoveTo(destinationCell);
			SetWaitMode();
			SwapSelectedUnit(null);
			destinationCell = null;
		}
	}
	private void DoNormalAttack(HexCell cell){
		if(selectedUnit &&  cell && cell.Unit){
			selectedUnit.Targets.Add(cell.Unit);
			selectedUnit.BasicAttackToTarget();
		}else{
			SwapSelectedUnit(null);
		}
		SetWaitMode();
	}
	
	
	private void DoAreaAttack(){
		selectedUnit.AreaAttackToTargets();
		SwapSelectedUnit(null);
		SetWaitMode();
	}
	
	void SwapSelectedUnit(HexUnit unit){
		previosSelected?.ClearHighlights();
		previosSelected = selectedUnit;
		selectedUnit = unit;
		HighlightSelectUnitAction();
	}
	
	bool UpdateCurrentCell () {
		HexCell cell =
			grid.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition));
		if (cell != currentCell ) {
			previosCell = currentCell;
			currentCell = cell;
			return true;
		}
		return false;
	}

	public void SetAttackMode(){
		if(selectedUnit){
			unitActionType = UnitActionsEnum.Attack;
			colorActiveAction = actionsColors[1];
			colorActiveActionHovered = actionColorHovered[1];
		}
		HighlightSelectUnitAction();
	}
	public void SetMoveMode(){
		if(selectedUnit){
			unitActionType = UnitActionsEnum.Move;
			colorActiveAction = actionsColors[0];
			colorActiveActionHovered = actionColorHovered[0];
		}
		HighlightSelectUnitAction();
	}

	public void SetWaitMode(){
		unitActionType = UnitActionsEnum.Wait;
		colorActiveAction = actionsColors[2];
		colorActiveActionHovered = actionColorHovered[2];
		HighlightSelectUnitAction();
	}

	
	public void UpdateUI(){
		TurnsText.text = Turns<10 ? $"0{Turns}":$"{Turns}";
		setSelectedUi();
	}
	public void setSelectedUi(){
		if(selectedUnit){
			selectInfo.gameObject.SetActive(true);
			selectInfo.SetUnit(selectedUnit);
		}else{
			selectInfo.gameObject.SetActive(false);
		}
	}
	

	
}