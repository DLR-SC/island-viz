using UnityEngine;
using UnityEngine.Networking;

public class BallPlayer : NetworkBehaviour {

	const int nudgeAmount = 33;
	
	public enum NudgeDir
	{
		Up,
		Down,
		Left,
		Right,
		Jump
	}

	void Start()
	{
	}
	
	[ClientCallback]
	void Update ()
	{
		if (!isLocalPlayer)
			return;
			
		if (Input.GetKey(KeyCode.Space))
		{
			CmdNudge(NudgeDir.Jump);
		}
		
		if (Input.GetKey(KeyCode.LeftArrow))
		{
			CmdNudge(NudgeDir.Left);
		}
		
		if (Input.GetKey(KeyCode.RightArrow))
		{
			CmdNudge(NudgeDir.Right);
		}
		
		if (Input.GetKey(KeyCode.UpArrow))
		{
			CmdNudge(NudgeDir.Up);
		}
		
		if (Input.GetKey(KeyCode.DownArrow))
		{
			CmdNudge(NudgeDir.Down);
		}
	}
	
	[Command]
	public void CmdNudge(NudgeDir direction)
	{
		switch (direction)
		{
			case NudgeDir.Left:
				GetComponent<Rigidbody>().AddForce(new Vector3(-nudgeAmount,0,0));
				break;
		
			case NudgeDir.Right:
				GetComponent<Rigidbody>().AddForce(new Vector3(nudgeAmount,0,0));
				break;

			case NudgeDir.Up:
				GetComponent<Rigidbody>().AddForce(new Vector3(0,0,nudgeAmount));
				break;
				
			case NudgeDir.Down:
				GetComponent<Rigidbody>().AddForce(new Vector3(0,0,-nudgeAmount));
				break;
				
			case NudgeDir.Jump:
				GetComponent<Rigidbody>().AddForce(new Vector3(0,nudgeAmount,0));
				break;
		}
	}
	
}
