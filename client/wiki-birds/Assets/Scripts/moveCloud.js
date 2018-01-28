    var speed: float;
    var offscreen: float;
    var spawn: float;
    var highest: float;
    var lowest: float;
     
     
    function Update () {
		//amount to move cloud
		var amtToMove = Time.deltaTime *speed;
		//move enemy
		transform.Translate(Vector3.right * amtToMove);
		//respawn with random Y
		if (transform.position.x > offscreen) {
       
			transform.position.x = spawn;
			transform.position.y = Random.Range (highest , lowest);
     
			}
    }