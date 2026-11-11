namespace DeliveryApp
{
    public enum PaymentType
    {
        Cash,
        Card
    }

    public class Order
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public double Weight { get; set; }
        public double Distance { get; set; }

        public Order(int id, string description, double weight, double distance)
        {
            Id = id;
            Description = description;
            Weight = weight;
            Distance = distance;
        }

        public override string ToString()
        {
            return $"Замовлення #{Id} ({Description}) - {Weight}кг, {Distance}км";
        }
    }

    public abstract class Transport
    {
        public string Name { get; set; }
        public bool IsBusy { get; set; }
        public Order CurrentOrder { get; set; }

        public Transport(string name)
        {
            Name = name;
            IsBusy = false;
            CurrentOrder = null;
        }

        public abstract double CalculateCost(Order order);

        public virtual bool CanTake(Order order)
        {
            return !IsBusy;
        }
    }

    public class Bike : Transport
    {
        public Bike(string name) : base(name) { }

        public override double CalculateCost(Order order)
        {
            return 20 + (5 * order.Distance);
        }

        public override bool CanTake(Order order)
        {
            if (order.Distance > 10)
            {
                return false;
            }

            return base.CanTake(order);
        }
    }

    public class Car : Transport
    {
        public Car(string name) : base(name) { }

        public override double CalculateCost(Order order)
        {
            return 50 + (8 * order.Distance) + (1 * order.Weight);
        }
    }

    public class Truck : Transport
    {
        public Truck(string name) : base(name) { }

        public override double CalculateCost(Order order)
        {
            return 200 + (15 * order.Distance) + (0.5 * order.Weight);
        }
    }

    public class DeliveryHistoryItem
    {
        public Order Order { get; set; }
        public Transport Transport { get; set; }
        public double Cost { get; set; }
        public PaymentType Payment { get; set; }

        public DeliveryHistoryItem(Order order, Transport transport, double cost, PaymentType payment)
        {
            Order = order;
            Transport = transport;
            Cost = cost;
            Payment = payment;
        }
    }

    public class DeliveryService
    {
        private List<Transport> transports = new List<Transport>();
        private List<Order> activeOrders = new List<Order>();
        private List<DeliveryHistoryItem> history = new List<DeliveryHistoryItem>();

        public void AddTransport(Transport t)
        {
            transports.Add(t);
        }

        public void AddOrder(Order o)
        {
            activeOrders.Add(o);
        }

        public void ShowCostsForOrder(Order order)
        {
            Console.WriteLine($"\n--- Вартість доставки для {order} ---");
            foreach (var t in transports)
            {
                if (t.CanTake(order))
                {
                    double cost = t.CalculateCost(order);
                    Console.WriteLine($"{t.Name}: {cost} грн");
                }
                else
                {
                    Console.WriteLine($"{t.Name}: не може виконати (обмеження за відстанню або зайнятий)");
                }
            }
        }

        public bool AssignOrder(Order order, Transport transport)
        {
            if (transport.CanTake(order))
            {
                transport.IsBusy = true;
                transport.CurrentOrder = order;
                Console.WriteLine($"\n[УСПІХ] Замовлення #{order.Id} призначено на {transport.Name}");
                return true;
            }
            else
            {
                Console.WriteLine($"\n[ПОМИЛКА] {transport.Name} не може взяти замовлення #{order.Id}");
                return false;
            }
        }

        public void CompleteDelivery(Transport transport, PaymentType payment)
        {
            if (transport.CurrentOrder == null)
            {
                Console.WriteLine($"\n{transport.Name} не має активного замовлення.");
                return;
            }

            Order order = transport.CurrentOrder;
            double cost = transport.CalculateCost(order);

            history.Add(new DeliveryHistoryItem(order, transport, cost, payment));
            activeOrders.Remove(order);

            transport.IsBusy = false;
            transport.CurrentOrder = null;

            string paymentStr = payment == PaymentType.Card ? "карткою" : "готівкою";
            Console.WriteLine($"\n[ДОСТАВЛЕНО] Замовлення #{order.Id} доставлено ({transport.Name}). Сума: {cost} грн, Оплата: {paymentStr}.");
        }

        public void PrintStatus()
        {
            Console.WriteLine("\n================ СТАТУС СИСТЕМИ ================");

            Console.WriteLine("\nАктивні замовлення:");
            if (activeOrders.Count == 0)
            {
                Console.WriteLine(" (немає)");
            }
            else
            {
                foreach (var o in activeOrders)
                {
                    Console.WriteLine($" - {o}");
                }
            }

            Console.WriteLine("\nІсторія доставок:");
            if (history.Count == 0)
            {
                Console.WriteLine(" (історія порожня)");
            }
            else
            {
                foreach (var item in history)
                {
                    string pay = item.Payment == PaymentType.Card ? "Картка" : "Готівка";
                    Console.WriteLine($" - Замовлення #{item.Order.Id} -> Транспорт: {item.Transport.Name} | Вартість: {item.Cost} грн | Оплата: {pay}");
                }
            }
            Console.WriteLine("================================================\n");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            DeliveryService service = new DeliveryService();

            Bike bike = new Bike("Велосипед #1");
            Car car = new Car("Автомобіль #1");
            Truck truck = new Truck("Вантажівка #1");

            service.AddTransport(bike);
            service.AddTransport(car);
            service.AddTransport(truck);

            Order order1 = new Order(1, "Документи", 0.5, 5.0);
            Order order2 = new Order(2, "Ноутбук", 3.0, 15.0);
            Order order3 = new Order(3, "Диван", 60.0, 8.0);

            service.AddOrder(order1);
            service.AddOrder(order2);
            service.AddOrder(order3);

            service.PrintStatus();

            service.ShowCostsForOrder(order1);
            service.ShowCostsForOrder(order2);

            service.AssignOrder(order2, bike);
            service.AssignOrder(order1, bike);
            service.AssignOrder(order3, bike);
            service.AssignOrder(order2, car);

            service.CompleteDelivery(bike, PaymentType.Card);
            service.CompleteDelivery(car, PaymentType.Cash);

            service.PrintStatus();

            Console.ReadKey();
        }
    }
}