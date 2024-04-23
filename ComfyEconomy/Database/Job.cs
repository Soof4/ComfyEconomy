namespace ComfyEconomy
{
    public class Job
    {
        public int JobID { get; set; }
        public string Owner { get; set; }
        public int ItemID { get; set; }
        public int Stack { get; set; }
        public int Payment { get; set; }
        public bool Active { get; set; }

        public Job(int jobID, string owner, int itemID, int stack, int payment, bool active)
        {
            JobID = jobID;
            Owner = owner;
            ItemID = itemID;
            Stack = stack;
            Payment = payment;
            Active = active;
        }
    }
}