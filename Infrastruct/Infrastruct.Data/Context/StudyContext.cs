using Domain.Models;
using Infrastruct.Data.Mappings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastruct.Data.Context
{
    /// <summary>
    /// 定义核心子领域——学习上下文
    /// </summary>
    public class StudyContext : DbContext
    {
        public DbSet<Student> Students { get; set; }

        /// <summary>
        /// 重写自定义Map配置
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //对 StudentMap 进行配置
            modelBuilder.ApplyConfiguration(new StudentMap());

            base.OnModelCreating(modelBuilder);
        }

        ///// <summary>
        ///// 重写连接数据库
        ///// </summary>
        ///// <param name="optionsBuilder"></param>
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    // 从 appsetting.json 中获取配置信息
        //    var config = new ConfigurationBuilder()
        //        .SetBasePath(Directory.GetCurrentDirectory())
        //        .AddJsonFile("appsettings.json")
        //        .Build();

        //    //定义要使用的数据库
        //    //正确的是这样，直接连接字符串即可
        //    //optionsBuilder.UseSqlServer(config.GetConnectionString("DefaultConnection"));
        //    //我是读取的文件内容，为了数据安全
        //    optionsBuilder.UseSqlServer(File.ReadAllText(config.GetConnectionString("DefaultConnection")));
        //}

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Data Source=.;Initial Catalog=OA;Persist Security Info=True;User ID=SA;Password=root1234;Encrypt=True;TrustServerCertificate=true");
    }
}
