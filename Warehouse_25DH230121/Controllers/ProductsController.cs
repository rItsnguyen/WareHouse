using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using Warehouse_25DH230121.Models;

namespace Warehouse_25DH230121.Controllers
{
    public class ProductsController : Controller
    {
        private Warehouse_25DH230121Entities1 db =
            new Warehouse_25DH230121Entities1();
        public ActionResult Index(string keyword, string category)
        {
            // Truy vấn sản phẩm bằng LINQ
            var products = db.Products.AsQueryable();

            // Tìm kiếm theo tên sản phẩm
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                products = products.Where(p =>
                    p.ProductName.Contains(keyword));
            }

            // Lọc sản phẩm theo danh mục
            if (!string.IsNullOrWhiteSpace(category))
            {
                products = products.Where(p =>
                    p.Category == category);
            }

            // Lấy danh sách danh mục
            ViewBag.Categories = new SelectList(
                db.Products
                    .Where(p => p.Category != null && p.Category != "")
                    .Select(p => p.Category)
                    .Distinct()
                    .ToList(),
                category
            );

            ViewBag.Keyword = keyword;
            ViewBag.SelectedCategory = category;

            return View(products.ToList());
        }
        // GET: Products/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Products product)
        {
            // Kiem tra ten san pham
            if (string.IsNullOrWhiteSpace(product.ProductName))
            {
                ModelState.AddModelError(
                    "ProductName",
                    "Ten san pham khong duoc de trong");
            }

            // Kiem tra danh muc
            if (string.IsNullOrWhiteSpace(product.Category))
            {
                ModelState.AddModelError(
                    "Category",
                    "Danh muc khong duoc de trong");
            }

            // Kiem tra don gia
            if (product.Price <= 0)
            {
                ModelState.AddModelError(
                    "Price",
                    "Don gia phai lon hon 0");
            }

            // Kiem tra so luong
            if (product.Quantity < 0)
            {
                ModelState.AddModelError(
                    "Quantity",
                    "So luong khong duoc am");
            }

            // Neu hop le thi luu vao Database
            if (ModelState.IsValid)
            {
                db.Products.Add(product);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            return View(product);
        }

        // Hiển thị trang nhập kho
        public ActionResult NhapKho()
        {
            ViewBag.Products = new SelectList(
                db.Products.ToList(),
                "ProductID",
                "ProductName"
            );

            return View();
        }

        // Xử lý nhập kho
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NhapKho(int productId, int quantity)
        {
            ViewBag.Products = new SelectList(
                db.Products.ToList(),
                "ProductID",
                "ProductName",
                productId
            );

            if (quantity <= 0)
            {
                ModelState.AddModelError(
                    "quantity",
                    "Số lượng nhập phải là số nguyên dương."
                );

                return View();
            }

            var product = db.Products.Find(productId);

            if (product == null)
            {
                ModelState.AddModelError(
                    "",
                    "Không tìm thấy sản phẩm."
                );

                return View();
            }

            // Cộng số lượng nhập vào tồn kho hiện tại
            product.Quantity += quantity;

            // Lưu vào CSDL
            db.SaveChanges();

            TempData["Message"] = "Nhập kho thành công!";

            return RedirectToAction("Index");
        }

        // Hiển thị trang xuất kho
        public ActionResult XuatKho()
        {
            ViewBag.Products = new SelectList(
                db.Products.ToList(),
                "ProductID",
                "ProductName"
            );

            return View();
        }

        // Xử lý xuất kho
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult XuatKho(int productId, int quantity)
        {
            ViewBag.Products = new SelectList(
                db.Products.ToList(),
                "ProductID",
                "ProductName",
                productId
            );

            // Kiểm tra số lượng xuất
            if (quantity <= 0)
            {
                ModelState.AddModelError(
                    "quantity",
                    "Số lượng xuất phải là số nguyên dương."
                );

                return View();
            }

            // Tìm sản phẩm
            var product = db.Products.Find(productId);

            if (product == null)
            {
                ModelState.AddModelError(
                    "",
                    "Không tìm thấy sản phẩm."
                );

                return View();
            }

            // Kiểm tra tồn kho
            if (quantity > product.Quantity)
            {
                ModelState.AddModelError(
                    "",
                    "Số lượng xuất vượt quá số lượng tồn kho."
                );

                return View();
            }

            // Trừ số lượng tồn kho
            product.Quantity -= quantity;

            // Lưu thay đổi vào CSDL
            db.SaveChanges();

            // Thông báo trạng thái tồn kho

            if (product.Quantity == 0)
            {
                TempData["Message"] =
                    "Xuất kho thành công! Sản phẩm đã hết hàng.";
            }
            else if (product.Quantity <= 5)
            {
                TempData["Message"] =
                    "Xuất kho thành công! Sắp hết hàng.";
            }
            else
            {
                TempData["Message"] =
                    "Xuất kho thành công!";
            }

            return RedirectToAction("Index");
        }

        // Hiển thị trang xuất hai sản phẩm
        public ActionResult XuatNhieuSanPham()
        {
            ViewBag.Products1 = new SelectList(
                db.Products.ToList(),
                "ProductID",
                "ProductName"
            );

            ViewBag.Products2 = new SelectList(
                db.Products.ToList(),
                "ProductID",
                "ProductName"
            );

            return View();
        }


        // Xử lý xuất hai sản phẩm trong cùng giao dịch
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult XuatNhieuSanPham(
            int productId1,
            int quantity1,
            int productId2,
            int quantity2)
        {
            // Nạp lại danh sách sản phẩm
            ViewBag.Products1 = new SelectList(
                db.Products.ToList(),
                "ProductID",
                "ProductName",
                productId1
            );

            ViewBag.Products2 = new SelectList(
                db.Products.ToList(),
                "ProductID",
                "ProductName",
                productId2
            );

            // Kiểm tra số lượng xuất
            if (quantity1 <= 0 || quantity2 <= 0)
            {
                ModelState.AddModelError(
                    "",
                    "Số lượng xuất phải là số nguyên dương."
                );

                return View();
            }

            // Kiểm tra hai sản phẩm phải khác nhau
            if (productId1 == productId2)
            {
                ModelState.AddModelError(
                    "",
                    "Phải chọn hai sản phẩm khác nhau."
                );

                return View();
            }


            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // Tìm hai sản phẩm
                    var product1 = db.Products.Find(productId1);
                    var product2 = db.Products.Find(productId2);

                    // Kiểm tra sản phẩm tồn tại
                    if (product1 == null || product2 == null)
                    {
                        transaction.Rollback();

                        ModelState.AddModelError(
                            "",
                            "Không tìm thấy sản phẩm."
                        );

                        return View();
                    }

                    // Kiểm tra hai sản phẩm có đủ hàng không
                    if (product1.Quantity < quantity1 ||
                        product2.Quantity < quantity2)
                    {
                        transaction.Rollback();

                        ModelState.AddModelError(
                            "",
                            "Không đủ hàng. Giao dịch đã bị hủy."
                        );

                        return View();
                    }

                    // Trừ số lượng của cả hai sản phẩm
                    product1.Quantity -= quantity1;
                    product2.Quantity -= quantity2;

                    // Lưu thay đổi
                    db.SaveChanges();

                    // Xác nhận giao dịch
                    transaction.Commit();

                    TempData["Message"] =
                        "Xuất hai sản phẩm thành công!";

                    return RedirectToAction("Index");
                }
                catch (Exception)
                {
                    transaction.Rollback();

                    ModelState.AddModelError(
                        "",
                        "Có lỗi xảy ra. Giao dịch đã được hủy."
                    );

                    return View();
                }
            }
        }

// Hiển thị trang chỉnh sửa sản phẩm
public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            var product = db.Products.Find(id);

            if (product == null)
            {
                return HttpNotFound();
            }

            return View(product);
        }


        // Xử lý cập nhật sản phẩm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Products model)
        {
            var product = db.Products.Find(id);

            // Kiểm tra sản phẩm có tồn tại không
            if (product == null)
            {
                return HttpNotFound();
            }

            // Kiểm tra dữ liệu
            if (string.IsNullOrWhiteSpace(model.ProductName))
            {
                ModelState.AddModelError(
                    "ProductName",
                    "Tên sản phẩm không được để trống."
                );
            }

            if (string.IsNullOrWhiteSpace(model.Category))
            {
                ModelState.AddModelError(
                    "Category",
                    "Danh mục không được để trống."
                );
            }

            if (model.Price <= 0)
            {
                ModelState.AddModelError(
                    "Price",
                    "Đơn giá phải lớn hơn 0."
                );
            }

            if (ModelState.IsValid)
            {
                // Chỉ cập nhật 3 trường được phép sửa
                product.ProductName = model.ProductName;
                product.Category = model.Category;
                product.Price = model.Price;

                // Không cập nhật Quantity và ProductID

                db.SaveChanges();

                TempData["Message"] =
                    "Cập nhật sản phẩm thành công!";

                return RedirectToAction("Index");
            }

            return View(model);
        }

        public ActionResult ThongKe()
        {
            // Tổng số sản phẩm
            ViewBag.TongSoSanPham = db.Products.Count();

            // Số sản phẩm hết hàng
            ViewBag.SoSanPhamHetHang =
                db.Products.Count(p => p.Quantity == 0);

            // Tổng giá trị tồn kho
            ViewBag.TongGiaTriTonKho =
                db.Products.Sum(p => (decimal?)(p.Price * p.Quantity)) ?? 0m;

            return View();
        }
    }
}