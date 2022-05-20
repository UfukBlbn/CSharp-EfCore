using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using ConsoleApp.Data.EfCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ConsoleApp
{     
    public class ShopContext: DbContext
    {        
        public DbSet<Product> Products {get;set;}
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders {get; set;}
        public DbSet<User> Users {get;set;}
        public DbSet<Address> Addresses {get;set;}
        public DbSet<Customer> Customers {get;set;}
        public DbSet<Supplier> Suppliers {get;set;}
        public static readonly ILoggerFactory MyLoggerFactory
            = LoggerFactory.Create(builder => { builder.AddConsole(); });   
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseLoggerFactory(MyLoggerFactory)
                //.UseSqlite("Data Source=shop.db");
                //.UseSqlServer(@"Data Source=.\SQLEXPRESS;Initial Catalog=ShopDb;Integrated Security=SSPI;");
                .UseMySql(@"server=localhost;port=3306;database=ShopDb;user=root;password=Blbnlr.123;");                
        }
        //Fluent Api
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                        .HasIndex(u=>u.Username)
                        .IsUnique();
            
            modelBuilder.Entity<ProductCategory>()
                        .HasKey(t=> new {t.ProductId,t.CategoryId});

            modelBuilder.Entity<ProductCategory>()
                        .HasOne(pc=>pc.Product)
                        .WithMany(p=>p.ProductCategories)
                        .HasForeignKey(pc=>pc.ProductId);

            modelBuilder.Entity<ProductCategory>()
                        .HasOne(pc=>pc.Category)
                        .WithMany(c=>c.ProductCategories)
                        .HasForeignKey(pc=>pc.CategoryId);

            modelBuilder.Entity<Customer>()
                        .Property(p=>p.IdentityNumber)
                        .IsRequired()
                        .HasMaxLength(11);
        }
    }
//User Address 1-M
//User Customer 1-1 // User Supplier 1-1

    public static class DataSeeding
    {
        public static void Seed(DbContext context)
        {
            if(context.Database.GetPendingMigrations().Count()==0)
            {
                if(context is ShopContext)
                {
                     ShopContext _context = context as ShopContext;
                
                    if(_context.Products.Count()==0)
                    {
                        _context.Products.AddRange(Products);
                    }

                    if(_context.Categories.Count()==0)
                    {
                        _context.Categories.AddRange(Categories);
                    }
                }
               context.SaveChanges();
            }
            
        }

        private static Product[] Products=
        {
            
             new Product(){Name="Iphone 12 ",Price=12400},
             new Product(){Name="Xiaomi Mi Band",Price=460},
             new Product(){Name="Iphone 13 ",Price=22400},
             new Product(){Name="Samsung Galaxy S12 ",Price=11400}

        };
        private static Category[] Categories =
        { 
        new Category() {Name="Phone"},
        new Category() {Name="Electronic"},
        new Category() {Name="Computer"}
        };
    }
    public class User 
    {
    public int Id { get; set; }
    [Required]
    [MaxLength(15),MinLength(8)]
    public string Username { get; set; }
    public string Email { get; set; }
    public Customer Customer { get; set; }
    public List<Address> Addresses { get; set; }
    }
    public class Customer
    {
        public int Id { get; set; }
        [Required]
        public string IdentityNumber { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [NotMapped]
        public string FullName { get; set; }
        public User User { get; set;}
        public int UserId { get; set; }
    }
    public class Supplier
    {
        public int Id { get; set; }
        public string SupName { get; set; }
        public string TaxNumber { get; set; }
    }
    public class Address
{
    public int Id { get; set; }
    public string fullName { get; set; }
    public string Title { get; set; }
    public string Body { get; set; }
    public User User { get; set; }
    public int UserId { get; set; }
}
    public class Product

    { 
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime InsertedDate { get; set; } = DateTime.Now;
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdatedDate { get; set; } = DateTime.Now;
        public List<ProductCategory> ProductCategories {get;set;}
    }
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<ProductCategory> ProductCategories {get;set;}
    }
    public class ProductCategory
    {
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
    }
    public class Order 
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public DateTime DateAdded { get; set; }
        
    }

    public class CustomerOrder
    {
        public CustomerOrder()
        {
            totalOrder= new List<TotalOrderPrice>();
        }
        public int Id { get; set; }
        public string CustomerFullName { get; set; }
        public int OrderCount { get; set; }
        public List<TotalOrderPrice> totalOrder { get; set; }
    }

    public class TotalOrderPrice
    {
        public int OrderId { get; set; }
        public decimal OrderPrice { get; set; }
        public List<ProductOrder> productorders {get;set;}
    }

    public class ProductOrder
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
    }
 
    class Program
    {
        static void Main(string[] args)
        {
            MoreThanOneTableOp();
        }

        static void MoreThanOneTableOp()
        {
            using(var db = new NorthwindContext())
            {
                var customersOrder = db.Customers.Where(i=>i.Orders.Count()>0)
                                                 .Select(i=> new CustomerOrder(){
                                                    CustomerFullName = i.FirstName,
                                                    Id = i.Id,
                                                    OrderCount = i.Orders.Count(),
                                                    totalOrder = i.Orders
                                                 .Select(s=> new TotalOrderPrice{
                                                        OrderId = s.Id,       
                                                        OrderPrice = (decimal)s.OrderDetails.Sum(s=>s.UnitPrice*s.Quantity),  

                                                        productorders = s.OrderDetails.Select(s=> new ProductOrder{
                                                            ProductId=s.Id,
                                                            ProductName=s.Product.ProductName
                                                        }).ToList()
                                                    }).ToList()
                                                 }) 
                                                 .OrderBy(i=>i.OrderCount)
                                                 .ToList();

                foreach (var cstm_order in customersOrder)
                {
                    Console.WriteLine($"ID : {cstm_order.Id} - Name : {cstm_order.CustomerFullName} - Total Order : {cstm_order.OrderCount}");

                    foreach (var order_price in cstm_order.totalOrder)
                    {
                        Console.WriteLine($"Order ID : {order_price.OrderId} - Order Price : {order_price.OrderPrice} ");

                        foreach (var product_name in order_price.productorders)
                        {
                            Console.WriteLine($"Id Of Product : {product_name.ProductId} - Name Of a Product {product_name.ProductName}");
                        }
                    }
                }
            }
        }

        static void SqlOperations()
        {
            
            // using(var db = new ShopContext())
            // {
            //     // var p = new Product(){
            //     //     Name="Casper",
            //     //     Price=12500
            //     // };
            //     // var p = db.Products.FirstOrDefault();
            //     // p.Name="Xiaomi";
            //     // db.Products.Update(p)
                
            // }

            // DataSeeding.Seed(new ShopContext());

            
            using(var db = new NorthwindContext()){
                
                //*******************************************************************************
                //Q-1 All customer's data
                var ctrs1 = db.Customers.ToList();
                foreach (var ct1 in ctrs1)
                {
                    //Console.WriteLine(ct1.FirstName+" "+ ct1.LastName);
                }

                
                //*******************************************************************************
                //Q-2 Only first_name and last_name***********
                var ctr2 = db.Customers.Select(c=>new{
                    c.FirstName,
                    c.LastName
                });
                foreach (var ct2 in ctr2)
                {
                    //Console.WriteLine(ct2.FirstName+" "+ ct2.LastName);
                }

                
                //*******************************************************************************
                //Q-3 NewYork Customers
                var ctr3 = db.Customers.Where(c=>c.City=="New York")
                                       .Select(s=>new{s.FirstName,s.LastName})
                                       .ToList();

                 foreach (var ct3 in ctr3)
                {
                   // Console.WriteLine(ct3.FirstName+" "+ ct3.LastName);
                }


                //*******************************************************************************
                //Q-4 Baverages name from category
                 var ctr4 = db.Products.Where(p=>p.Category=="Beverages")
                                       .Select(s1=>new{s1.ProductName,s1.ListPrice})
                                       .ToList();

                 foreach (var ct4 in ctr4)
                {
                    //Console.WriteLine(ct4.ProductName+" "+ ct4.ListPrice);
                }

                //*******************************************************************************
                //Q-5 Take 5 item from last part of the list

                var takeItem = db.Products.OrderByDescending(i=>i.Id).Take(5).ToList();

                foreach (var takenItem in takeItem)
                {
                    //Console.WriteLine(takenItem.ProductName+" "+ takenItem.ListPrice);
                }

                //*******************************************************************************
                //Q-6 Find the ıtems that between 10$ and 30$ cost. 

                var productRange = db.Products.Where(r=>r.ListPrice>=10 && r.ListPrice<=30)
                                              .Select(s=> new {s.ProductName,s.ListPrice})
                                              .ToList();

                //*******************************************************************************
                //Q-7 Count the items from the Beverages Category
                foreach (var rangeValue in productRange)
                {
                    //Console.WriteLine(rangeValue.ProductName+"  "+rangeValue.ListPrice);
                }

               
                //*******************************************************************************
                //Q-8 Find averages of the items from the Beverages Category
                var averageOfProduct = db.Products.Where(n=>n.Category=="Beverages").Average(a=>a.ListPrice);
                // Console.WriteLine(averageOfProduct);
             
             
               //*******************************************************************************
               //Q-9 Count Beverages Category
                var countOfBeverages = db.Products.Count(c=>c.Category=="Beverages");
                //Console.WriteLine("Counted items :  "+countOfBeverages);

               //*******************************************************************************
               //Q-10 USİNG SUM 
               var findCategory = db.Products.Where(w1=>w1.Category=="Beverages" || w1.Category=="Condiments")
                                             .Sum(w1=>w1.ListPrice);
                //Console.WriteLine("Total Cost ="+ findCategory);

               //*******************************************************************************
               //Q-11 CONTAİNS

               var findTheWord = db.Products.Where(i=>i.ProductName.ToLower().Contains("Tea") || i.Description.Contains("Tea")).ToList();
               foreach (var word in findTheWord)
               {
                   //Console.WriteLine(word.ProductName+" "+word.Description);    
               }
               
               //*******************************************************************************
               //Q-12 MAX MİN
               
               var minPrice = db.Products.Min(i=>i.ListPrice);
               var maxPrice = db.Products.Max(i=>i.ListPrice);

               //Console.WriteLine("Maximum Price of The Products : "+ maxPrice );

               //Console.WriteLine("Minimum Price of The Products :"+ minPrice);


            }
        }
        
        static void AddCatProd()
        {
            using(var db = new ShopContext())
            {
                var product = new List<Product>()
                {
                    new Product() {Name="Iphone 11S",Price=12500},
                    new Product() {Name="Iphone 13S",Price=13500},
                    new Product() {Name="Huawei ",Price=5500},
                    new Product() {Name="Xiaomi",Price=3500}
                };
                db.Products.AddRange(product);

                var category = new List<Category>(){
                    new Category(){Name="Electronic"},
                    new Category(){Name="Phone"},
                    new Category(){Name="Computer"},
                };

                db.Categories.AddRange(category);
                

                int[] ids = new int[2]{1,2};
                var p = db.Products.Find(1);
                p.ProductCategories = ids.Select(cid=> new ProductCategory()
                {
                    CategoryId=cid,
                    ProductId=p.Id
                }).ToList();
                db.SaveChanges();
            }

        }
        static void AddCustomer()
        {
            using(var db = new ShopContext())
            {
            //  var customers = new Customer()
            //     {
            //         IdentityNumber="123231231",
            //         FirstName="Ufuk",
            //         LastName="Balaban",
            //         UserId=1
            //     };
            //     db.Add(customers);
            //     db.SaveChanges();
                var user = new User()
                {
                    Username="McConeman",
                    Email="coneman@gmail.com",
                    Customer = new Customer()
                    {
                        FirstName="McConeman",
                        LastName="Coneman",
                        IdentityNumber="1232131231"
                    }
                };
                db.Users.Add(user);
                db.SaveChanges();
            }
          
        }
        static void NavigationProp()
        {
         using(var db = new ShopContext())
            {
               var user = db.Users.FirstOrDefault(i=>i.Username=="Ufuk Balaban");
               if(user!=null)
               {
                   user.Addresses=new List<Address>();
                   user.Addresses.AddRange(
                       new List<Address>()
                       {
                         new Address(){fullName="Ufuk Balaban", Title="Home Address1", Body="In the front of the market When you arrived to top of the elm street",UserId=1 },
                         new Address(){fullName="Ufuk Balaban", Title="Home Address2", Body="In the front of the market When you arrived to top of the elm street",UserId=1 },
                         new Address(){fullName="Ufuk Balaban", Title="Home Address3", Body="In the front of the market When you arrived to top of the elm street",UserId=1 },
                       }
                   );
               }
               db.SaveChanges();
           }
        }
        static void InsertUsers()
        {
            var users = new List<User>()
            {
                new User() {Username="Ufuk Balaban", Email="ufukkblbnn@gmail.com"},
                new User() {Username="Harm Warter", Email="hrmwaert@gmail.com"},
                new User() {Username="Johnk Klikli", Email="jhonlkli@gmail.com"},
                new User() {Username="Rickie And Mortie", Email="RAM@gmail.com"},
                new User() {Username="Mordekay", Email="mrdky@gmail.com"}

            };

             using(var db = new ShopContext())
            {
                //Burada Users alanı ShopContext İçerisinden gelmektedir!!
                db.Users.AddRange(users);
                db.SaveChanges();
            }
        }
        static void InsertAdresses()
        {
            var adresses = new List<Address>()
            {
                new Address(){fullName="Ufuk Balaban", Title="Home Address", Body="In the front of the market When you arrived to top of the elm street",UserId=1 },
                new Address(){fullName="Mordekay", Title="Home Address", Body="In the front of the market When you arrived to top of the elm street",UserId=5 },
                new Address(){fullName="Johnk Klikli", Title="Home Address", Body="In the front of the market When you arrived to top of the elm street",UserId=3 },
                new Address(){fullName="Harm Warter", Title="Home Address", Body="In the front of the market When you arrived to top of the elm street",UserId=4 },
                new Address(){fullName="Ufuk Balaban", Title="Home Address2", Body="In the front of the market When you arrived to top of the elm street",UserId=1 },
            };

            using(var db = new ShopContext())
            {
                 db.Addresses.AddRange(adresses);
                 db.SaveChanges();
            }
        }
        static void DeleteProduct(int id)
        {
            using(var db = new ShopContext())
            {

                var p = new Product(){Id=6};

                // db.Products.Remove(p);
                db.Entry(p).State = EntityState.Deleted;
                db.SaveChanges();    

                // var p = db.Products.FirstOrDefault(i=>i.Id==id);

                // if (p!=null)
                // {
                //     db.Products.Remove(p);
                //     db.SaveChanges();

                //     Console.WriteLine("veri silindi");
                // }
            }
        }
        static void UpdateProduct()
        {
            using(var db = new ShopContext())
            {
                var p = db.Products.Where(i=>i.Id==1).FirstOrDefault();

                if(p!=null)
                {
                    p.Price = 2400;

                    db.Products.Update(p);
                    db.SaveChanges();
                }
            }

            // using(var db = new ShopContext())
            // {
            //     var entity = new Product(){Id=1};
                
            //     db.Products.Attach(entity);

            //     entity.Price = 3000;

            //     db.SaveChanges();
            // }   



            //   using(var db = new ShopContext())
            //   {
            //       // change tracking
            //       var p = db
            //                 .Products
            //                 //.AsNoTracking()
            //                 .Where(i=>i.Id==1)
            //                 .FirstOrDefault();

            //       if (p!=null)
            //       {
            //             p.Price *= 1.2m;
            //             db.SaveChanges();
            //             Console.WriteLine("güncelleme yapıldı.");

            //       }
            //   }  
        }
        static void GetProductByName(string name)
        {
            using(var context = new ShopContext())
            {
                var products = context
                                .Products
                                .Where(p => p.Name.ToLower().Contains(name.ToLower()))
                                .Select(p => 
                                        new {
                                            p.Name,
                                            p.Price
                                        })
                                .ToList();
             
                foreach (var p in products)
                {
                    Console.WriteLine($"name: {p.Name} price: {p.Price}");
                }
                
            }
        }
        static void GetProductById(int id)
        {
            using(var context = new ShopContext())
            {
                var result = context
                                .Products
                                .Where(p => p.Id == id)
                                .Select(p => 
                                        new {
                                            p.Name,
                                            p.Price
                                        })
                                .FirstOrDefault();
             

                
                Console.WriteLine($"name: {result.Name} price: {result.Price}");
                
            }
        }
        static void GetAllProducts()
        {
            using(var context = new ShopContext())
            {
                var products = context
                .Products
                .Select(p => 
                    new {
                        p.Name,
                        p.Price
                    })
                .ToList();

                foreach (var p in products)
                {
                    Console.WriteLine($"name: {p.Name} price: {p.Price}");
                }
            }
        }
        static void AddProducts()
        {
            using(var db = new ShopContext())
            {
                var products = new List<Product>()
                {
                    new Product { Name = "Samsung S6", Price=3000 },
                    new Product { Name = "Samsung S7", Price=4000 },
                    new Product { Name = "Samsung S8", Price=5000 },
                    new Product { Name = "Samsung S9", Price=6000 }
                };          

                db.Products.AddRange(products);                
                db.SaveChanges();

                Console.WriteLine("veriler eklendi.");
            }
        }
        static void AddProduct()
        {
            using(var db = new ShopContext())
            {
                var p = new Product { Name = "Samsung S10", Price=8000 }; 

                db.Products.Add(p);                
                db.SaveChanges();

                Console.WriteLine("veriler eklendi.");
            }
        }
   
    }
}
