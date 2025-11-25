using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using TTCN.Models;

namespace TTCN.Controllers
{
    public class DatVeController : Controller
    {
        QLDVContext db = new QLDVContext();
        public IActionResult Index(int maPhim)
        {
            var dlPhim = (from tbPhim in db.Phims
                          join tbPhimTheLoai in db.PhimTheLoais on tbPhim.MaPhim equals tbPhimTheLoai.MaPhim
                          join tbTheLoai in db.TheLoais on tbPhimTheLoai.MaTheLoai equals tbTheLoai.MaTheLoai
                          join tbUsersPhim in db.UsersPhims on tbPhim.MaPhim equals tbUsersPhim.MaPhim
                          where tbPhim.MaPhim == maPhim
                          select new 
                          {
                              tbPhim.MaPhim,
                              tbPhim.TenPhim,
                              tbPhim.PosterPhim,
                              tbPhim.ThoiLuong,
                              tbPhim.NgayPhatHanh,
                              tbPhim.NgayKetThuc,
                              tbTheLoai.TenTheLoai
                          }).ToList();
            var ngayPhim = dlPhim.Where(p => p.MaPhim == maPhim).Select(p => new
            {
                p.NgayPhatHanh,
                p.NgayKetThuc
            }).FirstOrDefault();

            DateTime ngayBatDau = DateTime.Today > ngayPhim.NgayPhatHanh ? DateTime.Today : ngayPhim.NgayPhatHanh;

            int soNgay = (ngayPhim.NgayKetThuc - ngayBatDau).Days;

            List<DateTime> listNgay = new List<DateTime>();
            for (int i = 0; i < soNgay; i++)
            {
                DateTime ngay = ngayBatDau.AddDays(i);
                if (ngay <= ngayPhim.NgayKetThuc)
                {
                    listNgay.Add(ngay);
                }
            }

            var nhomPhim = from p in dlPhim
                           group p by new
                           {
                               p.MaPhim,
                               p.TenPhim,
                               p.PosterPhim,
                               p.ThoiLuong
                           } into g
                           select new DuLieuDatVe
                           {
                               MaPhim = g.Key.MaPhim,
                               TenPhim = g.Key.TenPhim,
                               PosterPhim = g.Key.PosterPhim,
                               ThoiLuong = g.Key.ThoiLuong,
                               CacNgayChieu = listNgay,
                               TenTheLoai = g.Select(x => x.TenTheLoai).Distinct().ToList()
                           };
            ViewBag.dlPhimDaChon = nhomPhim;

            var dlSuatChieu = db.Phims.Where(p=>p.MaPhim == maPhim)
                                      .Include(sc => sc.SuatChieus)
                                      .ThenInclude(pc => pc.MaPhongNavigation)
                                      .ThenInclude(cr=>cr.MaCumRapNavigation)
                                      .FirstOrDefault();
            ViewBag.dlSuatChieu = dlSuatChieu;
            return View();
        }
        [HttpGet]
        public IActionResult GetGheBySuatChieu(int maSuat)
        {
            // 1. Lấy thông tin Suất chiếu 
            var suat = db.SuatChieus
                .Include(s => s.MaPhongNavigation)
                .ThenInclude(p => p.GheNgois)
                .FirstOrDefault(s => s.MaSuat == maSuat);

            if (suat == null) return NotFound();

            // 2. Lấy danh sách ghế đã bán của suất này (trong bảng Ve)
            var gheDaBan = db.DonDatVes
                .Include(ddv => ddv.MaSuatNavigation)
                .ThenInclude(s => s.ChiTietScGnVes)
                .Where( s=> s.MaSuat == maSuat)
                .Select( v=>v.MaSuatNavigation.ChiTietScGnVes.Select(gg=>gg.MaGhe))
                .ToList();

            // 3. Map dữ liệu để trả về JSON
            // Sắp xếp theo Hàng (A, B, C) và Số ghế (1, 2, 3)
            var data = suat.MaPhongNavigation.GheNgois
                .OrderBy(g => g.HangGhe)
                .Select(g => new
                {
                    MaGhe = g.MaGhe,
                    TenGhe = g.TenGhe, // Ví dụ: A1
                    HangGhe = g.HangGhe,          // Để chia dòng
                    LoaiGhe = g.LoaiGhe,          // VIP/Thường // Tạm thời fix cứng hoặc lấy từ bảng GiaVe// True = Đã bán
                })
                .ToList();

            return Json(data);
        }

    }
}
