using System;
using System.Collections.Generic;

public enum LayOffCause
{
    VacationStockNegative,
    AgeAbove60,
    SalesTargetNotMet,
    Resigned
}

public class EmployeeLayOffEventArgs : EventArgs
{
    public LayOffCause Cause { get; set; }
}

class Employee
{
    public event EventHandler<EmployeeLayOffEventArgs> EmployeeLayOff;

    protected virtual void OnEmployeeLayOff(EmployeeLayOffEventArgs e)
    {
        EmployeeLayOff?.Invoke(this, e);
    }

    public int EmployeeID { get; set; }
    public DateTime BirthDate { get; set; }
    public int VacationStock { get; set; }

    public bool RequestVacation(DateTime from, DateTime to)
    {
        int daysRequested = (to - from).Days;
        if (VacationStock >= daysRequested)
        {
            VacationStock -= daysRequested;
            return true;
        }
        return false;
    }

    public virtual void EndOfYearOperation()
    {
        if (VacationStock < 0)
        {
            OnEmployeeLayOff(new EmployeeLayOffEventArgs { Cause = LayOffCause.VacationStockNegative });
        }

        int age = DateTime.Now.Year - BirthDate.Year;
        if (BirthDate > DateTime.Now.AddYears(-age)) age--;

        if (age > 60)
        {
            OnEmployeeLayOff(new EmployeeLayOffEventArgs { Cause = LayOffCause.AgeAbove60 });
        }
    }
}

class Department
{
    public int DeptID { get; set; }
    public string DeptName { get; set; }
    private List<Employee> Staff = new List<Employee>();

    public void AddStaff(Employee employee)
    {
        if (!Staff.Contains(employee))
        {
            Staff.Add(employee);
            Console.WriteLine($"Employee {employee.EmployeeID} added to Department.");
            employee.EmployeeLayOff += RemoveStaff;
        }
    }

    public void RemoveStaff(object sender, EmployeeLayOffEventArgs e)
    {
        var employee = sender as Employee;
        if (employee != null && Staff.Contains(employee))
        {
            if (e.Cause == LayOffCause.VacationStockNegative || e.Cause == LayOffCause.AgeAbove60)
            {
                Staff.Remove(employee);
                Console.WriteLine($"Employee {employee.EmployeeID} removed from Department.");
                employee.EmployeeLayOff -= RemoveStaff; 
            }
        }
    }
}

class Club
{
    public int ClubID { get; set; }
    public string ClubName { get; set; }
    private List<Employee> Members = new List<Employee>();

    public void AddMember(Employee e)
    {
        if (!Members.Contains(e))
        {
            Members.Add(e);
            Console.WriteLine($"Employee {e.EmployeeID} added to Department.");
            e.EmployeeLayOff += RemoveMember; 
        }
    }

    public void RemoveMember(object sender, EmployeeLayOffEventArgs e)
    {
        var employee = sender as Employee;
        if (employee != null && Members.Contains(employee))
        {
            if (e.Cause == LayOffCause.VacationStockNegative)
            {
                Members.Remove(employee);
                Console.WriteLine($"Employee {employee.EmployeeID} removed from Club.");
                employee.EmployeeLayOff -= RemoveMember; 
            }
        }
    }
}

class SalesPerson : Employee
{
    public int AchievedTarget { get; set; }

    public bool CheckTarget(int quota)
    {
        if (AchievedTarget < quota)
        {
            OnEmployeeLayOff(new EmployeeLayOffEventArgs { Cause = LayOffCause.SalesTargetNotMet });
            return false;
        }
        return true;
    }

    public override void EndOfYearOperation()
    {
        int age = DateTime.Now.Year - BirthDate.Year;
        if (BirthDate > DateTime.Now.AddYears(-age)) age--;

        if (age > 60)
        {
            return;
        }
    }
}

class BoardMember : Employee
{
    public void Resign()
    {
        OnEmployeeLayOff(new EmployeeLayOffEventArgs { Cause = LayOffCause.Resigned });
    }

    public override void EndOfYearOperation()
    {
    }
}

class Program
{
    public static void Main()
    {
        Department department = new Department { DeptID = 1, DeptName = "HR" };
        Club club = new Club { ClubID = 101, ClubName = "Employee Club" };

        Employee emp1 = new Employee { EmployeeID = 1, BirthDate = new DateTime(1950, 1, 1), VacationStock = -1 };
        Employee emp2 = new Employee { EmployeeID = 2, BirthDate = new DateTime(1990, 1, 1), VacationStock = 10 };

        department.AddStaff(emp1);
        department.AddStaff(emp2);
        club.AddMember(emp1);
        club.AddMember(emp2);

        emp1.EndOfYearOperation(); 
        emp2.EndOfYearOperation(); 

        emp1.VacationStock = -5;
        emp1.EndOfYearOperation(); 
    }
}
