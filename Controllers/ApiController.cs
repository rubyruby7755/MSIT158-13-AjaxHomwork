using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Text;
using website.Models;

namespace website.Controllers
{
    public class ApiController : Controller
    {
        private readonly MyDBContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ApiController( MyDBContext context, IWebHostEnvironment hostEnvironment)
        {  
            _context = context;
            _hostEnvironment = hostEnvironment;
        }
        public IActionResult Index()
        {
            //return View();
            //return Content("<h2>世界你好</h2>", "text/html");//看得懂html標籤，所以會顯示h2的樣式
            //return Content("世界你好", "text/plain"); //回傳純文字，但只看得懂英文，所以會出現亂碼
            System.Threading.Thread.Sleep(10000); //讓他執行速度慢一點
            return Content("世界你好", "text/plain", Encoding.UTF8); //中文就看得懂了
        }

        //讀出不會重複的城市名
        public IActionResult Cities()
        {
            var cities = _context.Addresses.Select(a => a.City).Distinct();//Distinct表示名稱不會重複
            return Json(cities);
        }

        //根據城市名讀出不會重複的鄉鎮區
        public IActionResult Districts(string city)
        {
            var districts = _context.Addresses.Where(a => a.City == city).Select(a => a.SiteId).Distinct();
            return Json(districts);
        }
        //根據鄉鎮區讀出路名
        public IActionResult Roads(string districts)
        {
            var roads = _context.Addresses.Where(a => a.SiteId == districts).Select(a => a.Road);
            return Json(roads);
        }


        public IActionResult Avatar(int id = 1)
        {
            Member? member = _context.Members.Find(id); 
            //Find方法，代表就是只搜尋主索引鍵，且為單一筆數的結構
            //Where方法，回傳的是IQueryable型態，也就是為多筆資料的結構
            //或是使用firstOrDefault 或 singleOrDefault 也會回傳單一筆資料
            if (member != null)
            {
                byte[] img = member.FileData;
                if (img != null)
                {
                    return File(img, "image/jpeg"); //後面那個參數是指會回傳甚麼類型的二進位資料
                }
            }
            return NotFound(); //找不到
        }

        //public IActionResult Register(int id,string name, int age = 20) //如果沒傳值進來就是預設20
        public IActionResult Register(Member member, IFormFile avatar)
        {
            if (string.IsNullOrEmpty(member.Name))
            {
                member.Name = "guest";
            }
            //取得上傳檔案的資訊
            //string info = $"{avatar.FileName} - {avatar.Length} - {avatar.ContentType}";



            //string info = _hostEnvironment.ContentRootPath;

            //檔案上傳
            //todo1 判斷檔案是否存在
            //todo2 限制上傳檔案的大小跟類型 
            //實際路徑
            //string uploadPath = @"C:\Users\User\Documents\workspace\MSIT158Site\wwwroot\uploads\a.png"; //這是寫死路徑，上網部屬後就不能用了
            //WebRootPath：C: \Users\User\Documents\workspace\MSIT158Site\wwwroot    //沒有完整的路徑，所以不夠
            //ContentRootPath：C:\Users\User\Documents\workspace\MSIT158Site         //不適用我們的專案環境

            string uploadPath = Path.Combine(_hostEnvironment.WebRootPath, "uploads", avatar.FileName);

            string info = uploadPath;
            using (var fileStream = new FileStream(uploadPath, FileMode.Create))
            {
                avatar.CopyTo(fileStream);

            }
            //using 用來建立一個區塊，在這個區塊中創建的 FileStream 物件會在作用域結束時自動關閉並釋放相關資源。
            //這是因為 FileStream 是一個實作了 IDisposable 介面的物件，因此需要在使用完畢後進行資源的釋放。

            //檔案上傳轉成二進位
            byte[] imgByte = null;
            using (var memoryStream = new MemoryStream())
            {
                avatar.CopyTo(memoryStream);
                imgByte = memoryStream.ToArray();
            }

            //寫進資料庫
            member.FileName = avatar.FileName;
            member.FileData = imgByte;
            _context.Members.Add(member);
            _context.SaveChanges();


            //return Content($"Hello {member.Name}，{member.Age} 歲了，電子郵件是 {member.Email}", "text/html", System.Text.Encoding.UTF8);
            return Content(info, "text/plain", System.Text.Encoding.UTF8);

        }

        [HttpPost]
        public IActionResult Spots([FromBody] SearchDTO searchDTO)
        {
            
                //根據分類編號搜尋景點資料
                var spots = searchDTO.categoryId == 0 ? _context.SpotImagesSpots : _context.SpotImagesSpots.Where(s => s.CategoryId == searchDTO.categoryId);

                //根據關鍵字搜尋景點資料(title、desc)
                if (!string.IsNullOrEmpty(searchDTO.keyword))
                {
                    spots = spots.Where(s => s.SpotTitle.Contains(searchDTO.keyword) || s.SpotDescription.Contains(searchDTO.keyword));
                }

                //排序
                switch (searchDTO.sortBy)
                {
                    case "spotTitle":
                        spots = searchDTO.sortType == "asc" ? spots.OrderBy(s => s.SpotTitle) : spots.OrderByDescending(s => s.SpotTitle);
                        break;
                    case "categoryId":
                        spots = searchDTO.sortType == "asc" ? spots.OrderBy(s => s.CategoryId) : spots.OrderByDescending(s => s.CategoryId);
                        break;
                    default:
                        spots = searchDTO.sortType == "asc" ? spots.OrderBy(s => s.SpotId) : spots.OrderByDescending(s => s.SpotId);
                        break;
                }





                //總共有多少筆資料
                int totalCount = spots.Count();
                //每頁要顯示幾筆資料
                int pageSize = searchDTO.pageSize;   //searchDTO.pageSize ?? 9;
                                                     //目前第幾頁
                int page = searchDTO.page;

                //計算總共有幾頁
                int totalPages = (int)Math.Ceiling((decimal)totalCount / pageSize);

                //分頁
                spots = spots.Skip((page - 1) * pageSize).Take(pageSize);


                //包裝要傳給client端的資料
                SpotsPagingDTO spotsPaging = new SpotsPagingDTO();
                spotsPaging.TotalCount = totalCount;
                spotsPaging.TotalPages = totalPages;
                spotsPaging.SpotsResult = spots.ToList();


                return Json(spotsPaging);
            }

        }







    }

