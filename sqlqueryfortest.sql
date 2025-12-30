--select * from sys.tables;
use OrderDb;
go
select count(*) as cnt,'InboxState' as tablename from InboxState
UNION
select count(*) as cnt,'Orders' as tablename from Orders
UNION
select count(*) as cnt,'OutboxState' as tablename from OutboxState
UNION
select count(*) as cnt,'OutboxMessage' as tablename from OutboxMessage


select  top 1 * from Orders ORDER BY CreatedAt desc 

select  * from InboxState ORDER BY Received desc

select * from OutboxMessage

select * from Orders where RazorpayOrderId like '%order_RxM4qzF4w7RLwY%'

