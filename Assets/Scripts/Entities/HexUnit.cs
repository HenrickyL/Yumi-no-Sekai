using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

public class HexUnit : MonoBehaviour {

	public Animator animator;
	public Transform body;
	public HexGrid grid;
	AnimationType animationType;
	HexDirectionAll animationDirection;
	const float rotationSpeed = 180f;
	bool live = true;
	public bool Dead { get{return !live;}}
	public  float TravelSpeed {
		get{
			return Speed*3;
		}
	}
	public List<HexCell> MovePath { 
		get{
			var response = location?.GetNeighborPerNivel(Speed, c=>IsValidDestination(c));
			return response;
		} 
	}
	public List<HexCell> AttackPath { 
		get{
			var response = location?.GetTriangleByDirection(direction,Range,c=>IsValidCellAttack(c,Range));
			return response;
		} 
	}
	List<HexUnit> attackTargets;
	List<HexUnit> oldAttackTargets;
	HexUnit target;
	List<HexUnit> _enemies;
	public List<HexUnit> Enemies {
		get{
			return _enemies;
		}
		set{
			_enemies = value.Where(x=>x!= this && !x.Dead).ToList();
		}}

	public List<HexUnit> OldAttackTargets { get{return oldAttackTargets;} }
	public List<HexUnit> Targets {
		get {
			return attackTargets;
		}
		set{
			if(value == null || !value.Any()){
				oldAttackTargets = attackTargets;
				attackTargets = null;
			}
			else{
				attackTargets = value;
				oldAttackTargets = attackTargets;
			}
		}
	}
	public int HP=450;
    public int MaxHp=500;
    public int Range = 3;
    public int Defense = 5;
    public int Speed = 2;
    public int Strength = 30;

	StatusBar _statusBar;
	
	public static HexUnit unitPrefab;
	public HexCell Location {
		get {
			return location;
		}
		set {
			if (location) {
				location.Unit = null;
			}
			oldLocation=location;
			location = value;
			value.Unit = this;
			transform.localPosition = value.Position;
		}
	}
	public HexCell OldLocation {
		get {
			return oldLocation;
		}
	}

	HexCell location;
	HexCell oldLocation;
	public AttackType attackType; 


	public float Orientation {
		get {
			return ((int)direction+1)*60f;
		}
		set {
			direction = (HexDirection)(value/60f);
			transform.localRotation = Quaternion.Euler(0f, Orientation, 0f);
		}
	}

	HexDirection direction;
	List<HexCell> pathToTravel;


	private void OnEnable() {
		_statusBar = gameObject.GetComponentInChildren<StatusBar>();
		RefreshStatusBar();
		animationType = AnimationType.Idle;
		animationDirection = HexDirectionAll.N;
		if (location) {
			transform.localPosition = location.Position;
		}
	}
	private UnitStatus GenUnitStatus(){
		var _status = new UnitStatus();
		_status.HP 		=	HP ;
		_status.MaxHp 	= 	MaxHp ;
		_status.Range 	= 	Range ;
		_status.Defense 	= 	Defense ;
		_status.Speed 	= 	Speed ;
		_status.Strength = 	Strength ;
		return _status;
	}

	private void UpdateStatusLocal(UnitStatus status){
		HP 		=	status.HP ;
		MaxHp 	= 	status.MaxHp ;
		Range 	= 	status.Range ;
		Defense = 	status.Defense ;
		Speed 	= 	status.Speed ;
		Strength= 	status.Strength ;
	}
	public void Destroy(){
		location.Unit = null;
		Destroy(gameObject);
	}
	private void RefreshStatusBar(){
		if(_statusBar)
			_statusBar.SetStatus(GenUnitStatus());
	}

	public void ValidateLocation () {
		transform.localPosition = location.Position;
	}

	public bool IsValidFullDestination (HexCell cell) {
		return cell && !cell.IsUnderwater && !cell.Unit;
	}
	public bool IsValidDestination (HexCell cell) {
		return  cell && !cell.IsUnderwater && !cell.Unit
				&& ( location.Elevation-1 <= cell.Elevation &&  cell.Elevation <= location.Elevation+1 );
	}
	public bool IsValidCellAttack (HexCell cell, int range=1) {
		return  cell &&( location.Elevation-range <= cell.Elevation &&  cell.Elevation < location.Elevation+range );
	}

	public void Travel (List<HexCell> path) {
		ClearHighlights();
		Location = path[path.Count - 1];
		pathToTravel = path;
		StopAllCoroutines();
		StartCoroutine(TravelPath());
	}

	IEnumerator TravelPath () {
		Vector3 a, b, c = pathToTravel[0].Position;
		transform.localPosition = c;
		SwapAnimationType(AnimationType.Walk);

		float t = Time.deltaTime * TravelSpeed;
		for (int i = 1; i < pathToTravel.Count; i++) {
			a = c;
			b = pathToTravel[i - 1].Position;
			c = (b + pathToTravel[i].Position) * 0.5f;
			for (; t < 1f; t += Time.deltaTime * TravelSpeed) {
				transform.localPosition = Bezier.GetPoint(a, b, c, t);
				Vector3 d = Bezier.GetDerivative(a, b, c, t);
				d.y = 0f;
				transform.localRotation = Quaternion.LookRotation(d);
				yield return null;
			}
			t -= 1f;
		}

		a = c;
		b = pathToTravel[pathToTravel.Count - 1].Position;
		c = b;
		for (; t < 1f; t += Time.deltaTime * TravelSpeed) {
			transform.localPosition = Bezier.GetPoint(a, b, c, t);
			Vector3 d = Bezier.GetDerivative(a, b, c, t);
			d.y = 0f;
			transform.localRotation = Quaternion.LookRotation(d);
			yield return null;
		}

		transform.localPosition = location.Position;
		Orientation = transform.localRotation.eulerAngles.y;
		ListPool<HexCell>.Add(pathToTravel);
		pathToTravel = null;
		SwapAnimationType(AnimationType.Idle);
	}
	

	// IEnumerator LookAt (Vector3 point) {
	// 	point.y = transform.localPosition.y;
	// 	Quaternion fromRotation = transform.localRotation;
	// 	Quaternion toRotation =
	// 		Quaternion.LookRotation(point - transform.localPosition);
	// 	float speed = rotationSpeed / Quaternion.Angle(fromRotation, toRotation);

	// 	for (
	// 		float t = Time.deltaTime * speed;
	// 		t < 1f;
	// 		t += Time.deltaTime * speed
	// 	) {
	// 		transform.localRotation =
	// 			Quaternion.Slerp(fromRotation, toRotation, t);
	// 		yield return null;
	// 	}

	// 	transform.LookAt(point);
	// 	Orientation = transform.localRotation.eulerAngles.y;
	// }

	public void Die() {
		ClearHighlights();
		SwapAnimationType(AnimationType.Die);
		live = false;
		grid.Units.Remove(this);
		this.enabled = false;
		Destroy(_statusBar);

	}
	

	public void Save (BinaryWriter writer) {
		location.coordinates.Save(writer);
	}

	public static void Load (BinaryReader reader, HexGrid grid) {
		HexCoordinates coordinates = HexCoordinates.Load(reader);
		float orientation = reader.ReadSingle();
		grid.AddUnit(
			Instantiate(unitPrefab), grid.GetCell(coordinates), orientation
		);
	}

	private void LateUpdate() {
		if(DieCondition()){
			Die();
		}
		CamAjust();
		RefreshStatusBar();
	}


    void CamAjust(){
		
		var cam = Camera.main.transform.position;
		var position = transform.position;
		var adjust = new Vector3(cam.x, position.y,cam.z);
		body.LookAt(adjust);
		UpdateAnimationDirection(cam);
	}

	public bool IsValidMoveDestination(HexCell cell){
		return MovePath.Contains(cell);
	}
	public bool IsValidAttackDestination(HexCell cell){
		return AttackPath.Contains(cell);
	}
	public bool IsOldTarget(HexUnit unit){
		return AttackPath.Contains(unit.Location);
	}

	public void ShakeUnit(){
		Vector3 position = this.transform.position;
		//https://forum.unity.com/threads/shake-an-object-from-script.138382/
	}

	public virtual bool BasicAttackTargets(){
		if(Targets !=null && Targets.Any()){
			foreach(var t in Targets){
				t.TakeDamage(CalcDamageNormalAttack(this));
			}
			return true;
		}
		return false;
	}

	protected virtual float  CalcDamageNormalAttack(HexUnit unit){
		return Strength*(1-Defense/100);
	}
	public  virtual void TakeDamage(float value){
		this.HP = (int)Math.Round(HP - value);
		RefreshStatusBar();
	}
	public  virtual void TakeHeal(float value){
		this.HP = (int)Math.Round(HP + value);
		RefreshStatusBar();
	}

	void UpdateAnimationDirection(Vector3 camPos){
		var position = transform.position;
		var pos = new Vector2(position.x, position.z);
		var cam = new Vector2(camPos.x, camPos.z);
		var v1 =  pos -Vector2.up;
		var v2 =  pos - cam;
		float angle = Mathf.Atan2(v2.x, v2.y) * Mathf.Rad2Deg;
		animationDirection = angle.HalfCircleToDirection();
		SwapAnimationType(animationType);
	}

	void SwapAnimationType(AnimationType type){
		if(type != animationType){
			animator.SetBool($"Idle",false);
			animator.SetBool($"Walk",false);
			animator.SetBool($"Die",false);
			animator.SetBool($"Attack",false);
			animationType = type;
			animator.SetBool($"{animationType}",true);
		}
		animator.SetBool($"{animationType}N", 
				animationDirection.IsFront());
		animator.SetBool($"{animationType}S",
				animationDirection.IsBack());
	}

	public virtual bool AutomaticTraverToEnemy(bool indicators = false){
		if(Enemies!=null && Enemies.Any() && grid){
			target = FindNearTarget(Enemies);
			if(target){
				var enemyNeighbors = target.Location.GetNeighbors();
				var cell = target.Location.GetNeighbor( (HexDirection) new System.Random().Next(0,6));
				Debug.Log(cell);
				if(indicators){
					target.Location.EnableHighlight(Color.cyan);
				}
				grid.ClearPath();
				grid.FindPath(location,cell,(int)TravelSpeed);
				var path = grid.GetPath();
				if(path!= null && path.Any()){
					var way = path.Where(x=>MovePath.Contains(x)).ToList();
					if(way.Any()){
						Travel(way);
						if(!indicators){
							grid.ClearPath();
						}
						return true;
					}else{
						location.EnableHighlight(Color.black);
					}
				}
			}
		}
		return false;
	}

	public virtual bool AutomaticAttackNearEnemy(){
		if(Enemies!=null && Enemies.Any() && grid){
			target = FindNearTargetWithLessLife(Enemies);
			if(target){
				var isAttackable = AttackPath.Contains(target.location);
				if(!isAttackable) return false;
				Targets = new List<HexUnit>(){target};
				return BasicAttackTargets();
			}
		}
		return false;
	}
	
	public virtual bool AutomaticAggressiveMovement(){
		bool heAttacked = false;
		DefineTarget();
		if(!target)
			return false;
		if( AttackPath!=null && AttackPath.Contains(target.location)){
			Targets = new List<HexUnit>(){target};
			heAttacked = BasicAttackTargets();
		}
		if(!heAttacked){
			return AutomaticTraverToEnemy();
		}else{
			return AutomaticAttackNearEnemy();
		}
	}
	void DefineTarget(){
		target = FindNearTarget(Enemies);
	}
	HexUnit FindNearTarget(List<HexUnit> options){
		if(options!= null && options.Any()){
			var position = transform.position;
			return options.Aggregate(
					(near,x)=>(near == null || 
						Vector3.Distance(position,x.transform.position) <  Vector3.Distance(position,near.transform.position) && !x.Dead)? x : near);
		}
		return null;
	}
	
	HexUnit FindNearTargetWithLessLife(List<HexUnit> options){
		var position = transform.position;
		return options.Aggregate(
				(nearLL,x)=>(nearLL == null || 
					Vector3.Distance(position,x.transform.position) <  Vector3.Distance(position,nearLL.transform.position) )
					&& x.HP < nearLL.HP  && !x.Dead? x : nearLL);
	}

	public void ClearHighlights(){
		ClearHighlightMove();
		ClearHighlightAttack();
		ClearHighlight();
	}
	public void ClearHighlightMove(){
		Location.DisableHighlight();
		foreach(var c in MovePath){
			c.DisableHighlight();
		}
	}
	public void ClearHighlightAttack(){
		Location.DisableHighlight();
		foreach(var c in MovePath){
			c.DisableHighlight();
		}
	}

	public void EnableHighlightAttack(Color color){
		Location.EnableHighlight(color);
		foreach(var c in AttackPath){
			c.EnableHighlight(color);
		}
	}

	public void EnableHighlight(Color color){
		Location.EnableHighlight(color);
	}
	public void ClearHighlight(){
		Location.DisableHighlight();
	}

	public virtual bool DieCondition(){
		return HP <= 0 && !Dead;
	}

	public bool MoveTo(HexCell cell){
		grid.ClearPath();
		grid.FindPath(location,cell,(int)TravelSpeed);
		var path = grid.GetPath();
		if(path.Any()){
			Travel(path);
			grid.ClearPath();
			return true;
		}else{
			grid.ClearPath();
			return false;
		}
	}

}

public static class HexUnitExtensions {
	public static List<HexCell> GetCells (this List<HexUnit> units) {
		return units.Select(u=> u.Location).ToList();
	}
	public static List<Transform> GetCellsTrasforms (this List<HexUnit> units) {
		return units.Select(u=> u.Location.transform).ToList();
	}
}