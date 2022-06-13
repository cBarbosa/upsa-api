namespace upsa_api.Models
{
    public class NotifyAvocadoModel
    {
        public string ProcessNumber { get; set; }
        public string InternalDate1
        {
            get => InternalDate1.Equals("null") ? "Sem Prazo" : InternalDate1;
            set => InternalDate1 = value;
        }
        public string CourtDate1
        {
            get => CourtDate1.Equals("null") ? "Sem Prazo" : CourtDate1;
            set => CourtDate1 = value;
        }
        public string InternalDate2
        {
            get => InternalDate2.Equals("null") ? "Sem Prazo" : InternalDate2;
            set => InternalDate2 = value;
        }
        public string CourtDate2
        {
            get => CourtDate2.Equals("null") ? "Sem Prazo" : CourtDate2;
            set => CourtDate2 = value;
        }
        public string Observation { get; set; }
    }
}