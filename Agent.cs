namespace Mazina_GlazkiSave
{
    using System;
    using System.Collections.Generic;

    public partial class Agent
    {
        public Agent()
        {
            this.AgentPriorityHistory = new HashSet<AgentPriorityHistory>();
            this.ProductSale = new HashSet<ProductSale>();
            this.Shop = new HashSet<Shop>();
        }

        public int ID { get; set; }
        public int AgentTypeID { get; set; }
        public string Title { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Logo { get; set; }
        public string Address { get; set; }
        public int Priority { get; set; }
        public string DirectorName { get; set; }
        public string INN { get; set; }
        public string KPP { get; set; }
        public string AgentTypeTitle
        {
            get
            {
                return AgentType.Title;
            }
        }

        public virtual AgentType AgentType { get; set; }
        public virtual ICollection<AgentPriorityHistory> AgentPriorityHistory { get; set; }
        public virtual ICollection<ProductSale> ProductSale { get; set; }
        public virtual ICollection<Shop> Shop { get; set; }

        public decimal Sales
        {
            get
            {
                decimal s = 0;
                foreach (ProductSale p in ProductSale)
                {
                    s += p.Stoimost;
                }
                return s;
            }

        }
        public int Discount
        {
            get
            {
                if (Sales >= 500000)
                    return 25;
                if (Sales >= 150000)
                    return 20;
                if (Sales >= 50000)
                    return 10;
                if (Sales >= 10000)
                    return 5;
                else
                    return 0;
            }
        }
        public int SalesForYears
        {
            get
            {
                int s = 0;
                foreach (ProductSale p in ProductSale)
                {
                    TimeSpan differentWithoutTime = DateTime.Today.Date - p.SaleDate.Date;
                    if ((int)differentWithoutTime.TotalDays <= 366)
                        s += p.ProductCount;
                }
                return s;
            }
        }
        public string FonStayle
        {
            get
            {
                if (Discount >= 25)
                    return "LightGreen";
                else
                    return "White";
            }
        }
    }
}