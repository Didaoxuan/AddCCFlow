﻿using Christ3D.Domain.Validations;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Commands.Order
{
    /// <summary>
    /// 注册一个添加 Order 命令
    /// 基础抽象学生命令模型
    /// </summary>
    public class RegisterOrderCommand : OrderCommand
    {
        // set 受保护，只能通过构造函数方法赋值
        public RegisterOrderCommand(string name, List<OrderItem> items)
        {
            Name = name;
            Items = items;
        }

        // 重写基类中的 是否有效 方法
        // 主要是为了引入命令验证 RegisterOrderCommandValidation。
        public override bool IsValid()
        {
            ValidationResult = new RegisterOrderCommandValidation().Validate(this);
            return ValidationResult.IsValid;
        }
    }
}
