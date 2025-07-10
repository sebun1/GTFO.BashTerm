namespace BashTerm.Runtime;

public class Thread {
	public int TID { get; }
	public Screen scrn;

	public Thread(int id) {
		TID = id;
		scrn = new Screen(id);
	}
}
