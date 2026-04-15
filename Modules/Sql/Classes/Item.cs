public class Item
{
    public int Id { get; set; }
    public string MMSID { get; set; }
    public string Barcode { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }

    public Item(int id, string mmsId, string barcode, string title, string description)
    {
        this.Id = id;
        this.MMSID = mmsId;
        this.Barcode = barcode;
        this.Title = title;
        this.Description = description;
    }

    public override string ToString()
    {
        return $"(Item: {this.Id}\t{this.MMSID}\t{this.Barcode}\t{this.Title}\t{this.Description}";
    }
}