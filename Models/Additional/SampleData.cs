using HelpDesk.Models.Articles;
using HelpDesk.Models.Clients;
using HelpDesk.Models.Common;
using HelpDesk.Models.Notifications;
using HelpDesk.Models.SLAs;
using HelpDesk.Models.Task;
using HelpDesk.Models.Tickets;
using HelpDesk.Models.Users;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HelpDesk.Models.Additional
{
    public static class SampleData
    {
        public static void Initialize(AppDbContext db)
        {
            if (!db.Tickets.Any())
            {
                var hasher = new PasswordHasher<AppUser>();
                var commonPass = hasher.HashPassword(new AppUser(), "123321");
                // Tickets
                db.TicketStatuses.AddRange(new List<TicketStatus>{
                    new TicketStatus()
                    {
                        Name = "Принят"
                    },
                    new TicketStatus()
                    {
                        Name = "В работе"
                    },
                    new TicketStatus()
                    {
                        Name = "Не подтвержден"
                    },
                    new TicketStatus
                    {
                        Name = "Закрыт"
                    },
                    new TicketStatus
                    {
                        Name = "Отклонен"
                    }
                });

                db.TicketTypes.AddRange(new List<TicketType>
                {
                    new TicketType()
                    {
                        Name = "Создание нового функционала"
                    },
                    new TicketType()
                    {
                        Name = "Решение проблем"
                    },
                    new TicketType()
                    {
                        Name = "Доработка старого функционала"
                    }
                });
                // Tickets

                // Common
                db.Priorities.AddRange(new List<Priority>
                {
                    new Priority()
                    {
                        Name = "Низкая"
                    },
                    new Priority()
                    {
                        Name = "Средняя"
                    },
                    new Priority()
                    {
                        Name = "Высокая"
                    }
                });

                db.Objectives.AddRange(new List<Objective>
                {
                    new Objective()
                    {
                        Name = "Students project"
                    },
                    new Objective()
                    {
                        Name = "EmailScheduler"
                    }
                });

                db.Departaments.AddRange(new List<Departament>
                {
                    new Departament()
                    {
                        Name = "Software отдел"
                    },
                    new Departament()
                    {
                        Name = "Hardware отдел"
                    },
                    new Departament()
                    {
                        Name = "Design отдел"
                    }
                });

                db.Roles.AddRange(new List<Role>
                {
                    new Role()
                    {
                        Name = "admin"
                    },
                    new Role()
                    {
                        Name = "superuser"
                    },
                    new Role()
                    {
                        Name = "user"
                    },
                    new Role
                    {
                        Name = "superclient"
                    },
                    new Role
                    {
                        Name = "client"
                    }
                });
                // Common
                db.SaveChanges();
                // SLAs
                db.SLAs.AddRange(new List<SLA>
                {
                    new SLA()
                    {
                        Name = "SLA 1"
                    },
                    new SLA()
                    {
                        Name = "SLA 2"
                    }
                });
                db.SaveChanges();
                db.SLAAllowedDepartaments.AddRange(new List<SLAAllowedDepartament>
                {
                    new SLAAllowedDepartament()
                    {
                        DepartamentId = 1,
                        SLAId = 1,
                        Unique = "1_1"
                    },
                    new SLAAllowedDepartament()
                    {
                        DepartamentId = 2,
                        SLAId = 2,
                        Unique = "2_2"
                    }
                });
                db.SLAAllowedObjectives.AddRange(new List<SLAAllowedObjective>()
                {
                    new SLAAllowedObjective()
                    {
                        ObjectiveId = 1,
                        SLAId = 1,
                        Unique = "1_1"
                    },
                    new SLAAllowedObjective()
                    {
                        ObjectiveId = 2,
                        SLAId = 2,
                        Unique = "2_2"
                    }
                });
                db.SLAAllowedPriorities.AddRange(new List<SLAAllowedPriority>()
                {
                    new SLAAllowedPriority()
                    {
                        PriorityId = 1,
                        SLAId = 1,
                        Unique = "1_1"
                    },
                    new SLAAllowedPriority()
                    {
                        PriorityId = 2,
                        SLAId = 2,
                        Unique = "2_2"
                    }
                });
                db.SLAAllowedTypes.AddRange(new List<SLAAllowedType>()
                {
                    new SLAAllowedType()
                    {
                        TypeId = 1,
                        SLAId = 1,
                        Unique = "1_1"
                    },
                    new SLAAllowedType()
                    {
                        TypeId = 2,
                        SLAId = 2,
                        Unique = "2_2"
                    }
                });
                db.SLATime.AddRange(new List<SLATime>()
                {
                    new SLATime()
                    {
                        WorkTime = 180,
                        ResponseTime = 60,
                        SLAId = 1,
                        PriorityId = 1,
                        Unique = "1_1"
                    }
                });
                // SLAs
                db.SaveChanges();
                // Clients
                db.Companies.AddRange(new List<Company>
                {
                    new Company()
                    {
                        SLAId = 1,
                        Name = "ES Systems"
                    },
                    new Company()
                    {
                        SLAId = 2,
                        Name = "Meiendorf Inc"
                    }
                });
                db.SaveChanges();
                db.Clients.AddRange(new List<Client>
                {
                    new Client()
                    {
                        Email = "romam2002@mail.ru",
                        Password = commonPass,
                        Phone = "+37368963798",
                        CompanyId = 1,
                        FullName = "Клиент из 1 компании",
                        RoleId = 4,
                    },
                    new Client()
                    {
                        Email = "shishocika4@mail.ru",
                        Password = commonPass,
                        Phone = "+37368963798",
                        CompanyId = 1,
                        FullName = "Суперклиент из 1 компании",
                        RoleId = 4,
                    },
                    new Client()
                    {
                        Email = "rororo2020@mail.ru",
                        Password = commonPass,
                        Phone = "+37368963799",
                        CompanyId = 2,
                        FullName = "Супекрилент из 2 компании",
                        RoleId = 5,
                    }
                });
                db.SaveChanges();
                // Clients

                // Users
                db.UserStatuses.AddRange(new List<UserStatus>()
                {
                    new UserStatus()
                    {
                        Name = "Свободен"
                    },
                    new UserStatus()
                    {
                        Name = "Занят"
                    },
                    new UserStatus()
                    {
                        Name = "Не активен"
                    }
                });

                db.Users.AddRange(new List<User>
                {
                    new User()
                    {
                        Email = "shishocika2@mail.ru",
                        Password = commonPass,
                        FullName = "Джей Ролл",
                        DepartamentId = 1,
                        RoleId = 1,
                        StatusId = 1
                    },
                    new User()
                    {
                        Email = "shishocika3@mail.ru",
                        Password = commonPass,
                        FullName = "Питер Пренк",
                        DepartamentId = 1,
                        RoleId = 2,
                        StatusId = 1
                    },
                    new User()
                    {
                        Email = "neo1110111@gmail.com",
                        Password = commonPass,
                        FullName = "Зигги Лю",
                        DepartamentId = 2,
                        RoleId = 3,
                        StatusId = 1
                    }
                });
                db.SaveChanges();
                // Articles
                db.ArticleTypes.AddRange(new List<ArticleType>
                {
                    new ArticleType()
                    {
                        Name = "Внутренняя"
                    },
                    new ArticleType()
                    {
                        Name = "Общедоступная"
                    }
                });
                db.ArticleSections.AddRange(new List<ArticleSection>
                {
                    new ArticleSection()
                    {
                        Name = "Тикеты"
                    },
                    new ArticleSection()
                    {
                        Name = "Задачи"
                    }
                });
                db.SaveChanges();
                db.Articles.Add(new Article()
                {
                    Content = "test",
                    Name = "test",
                    SectionId = 1,
                    TypeId = 2,
                    UserId = 1
                });

                // Events
                db.EventTypes.AddRange(new List<EventType>
                {
                    new EventType()
                    {
                        Name = "TicketAdded"
                    },
                    new EventType()
                    {
                        Name = "TicketChanged"
                    },
                    new EventType()
                    {
                        Name = "TicketCommented"
                    },
                    new EventType()
                    {
                        Name = "TaskAdded"
                    },
                    new EventType()
                    {
                        Name = "TaskChanged"
                    },
                    new EventType()
                    {
                        Name = "TaskCommented"
                    }
                });
                db.Events.Add(new Event()
                {
                    Date = DateTime.Now,
                    TypeId = 1,
                    Description = "Тикет '1' принят"
                });
                db.SaveChanges();
                db.Notifications.Add(new Notification()
                {
                    UseEmail = true,
                    ClientId = 1,
                    EventTypeId = 1
                });
                // Events

                // Ticket
                db.Tickets.AddRange(new List<Ticket>()
                {
                    new Ticket()
                    {
                        Name = "Test 1",
                        ClientId = 1,
                        StatusId = 1,
                        PriorityId = 3,
                        Content = "test ticket",
                        DateCreated = DateTime.Now,
                        DepartamentId = 1,
                        ObjectiveId = 1,
                        TypeId = 1,
                        UserInitId = 1
                    },
                    new Ticket()
                    {
                        Name = "Test 3",
                        ClientId = 1,
                        StatusId = 1,
                        PriorityId = 3,
                        Content = "test ticket 2",
                        DateCreated = DateTime.Now,
                        DepartamentId = 1,
                        ObjectiveId = 1,
                        TypeId = 1,
                        UserInitId = 1
                    }
                });
                db.SaveChanges();
                db.TicketComments.Add(new TicketComment()
                {
                    Content = "test ticket comment",
                    TicketId = 1,
                    UserId = 1
                });
                db.SaveChanges();
                //Task
                db.TaskStatuses.AddRange(new List<TicketTaskStatus>
                {
                    new TicketTaskStatus()
                    {
                        Name = "Открыто"
                    },
                    new TicketTaskStatus()
                    {
                        Name = "В работе"
                    },
                    new TicketTaskStatus()
                    {
                        Name = "Ожидание"
                    },
                    new TicketTaskStatus()
                    {
                        Name = "Завершено"
                    }
                });
                db.TicketTasks.Add(new TicketTask
                {
                    Name = "test ticket task",
                    Content = "test ticket task",
                    UserId = 2,
                    TicketId = 1,
                    DateCreated = DateTime.Now,
                    StatusId = 1
                });
                db.TicketTasks.Add(new TicketTask
                {
                    Name = "test ticket task 2",
                    Content = "test ticket task",
                    UserId = 1,
                    TicketId = 1,
                    DateCreated = DateTime.Now,
                    StatusId = 2
                });
                db.SaveChanges();
                db.TaskComments.Add(new TaskComment()
                {
                    Content = "test ticket comment",
                    TaskId = 1,
                    UserId = 2
                });
                db.SaveChanges();
                /*var verifyHasher = new PasswordHasher<AppUser>();
                Console.WriteLine(verifyHasher.VerifyHashedPassword(new AppUser(), db.Users.First().Password, "123321"));*/
            }
        }
    }
}
