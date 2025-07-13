namespace BashTerm.Sys;

public class Thread {
	public int TID { get; }
	public Screen scrn;

	public Thread(int id) {
		TID = id;
		scrn = new Screen(id);
	}
}
