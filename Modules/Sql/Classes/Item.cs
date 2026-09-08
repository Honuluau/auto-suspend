public class Item {
    public int Id { get; set; }
    public string MMSID { get; set; }
    public string Barcode { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }

    /// <summary>
    /// Constructor method for Item.
    /// </summary>
    /// <param name="id">Database Id.</param>
    /// <param name="mmsId">Inventory Id in Alma.</param>
    /// <param name="barcode">Barcode in Alma.</param>
    /// <param name="title">Title in Alma.</param>
    /// <param name="description">Description in Alma.</param>
    public Item(int id, string mmsId, string barcode, string title, string description) {
        this.Id = id;
        this.MMSID = mmsId;
        this.Barcode = barcode;
        this.Title = title;
        this.Description = description;
    }

    /// <summary>
    /// This method implements ToString for this class.
    /// </summary>
    /// <returns></returns>
    public override string ToString() {
        return $"({this.Title}, {this.Barcode})";
    }
}