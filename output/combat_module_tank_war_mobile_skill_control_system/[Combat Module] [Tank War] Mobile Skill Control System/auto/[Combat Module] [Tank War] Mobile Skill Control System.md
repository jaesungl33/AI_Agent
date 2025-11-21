# [Combat Module] [Tank War] Mobile Skill Control System

THIẾT KẾ HỆ THỐNG ĐIỀU KHIỂN KỸ NĂNG TRÊN MOBILE

# Version: v1.1

Người viết: $\textcircled{9}$ linhttd

Ngày cập nhật: 17 - 09 - 2025

<table><tr><td rowspan=1 colspan=1>Phienban</td><td rowspan=1 colspan=1>Ngay</td><td rowspan=1 colspan=1>M6ta</td><td rowspan=1 colspan=1>Nguoi viet</td><td rowspan=1 colspan=1>Nguoireview</td><td rowspan=1 colspan=1>Duyét？</td></tr><tr><td rowspan=1 colspan=1>v1.0</td><td rowspan=1 colspan=1>16-09-2025</td><td rowspan=1 colspan=1>Tao file</td><td rowspan=1 colspan=1> linhttd </td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>□</td></tr><tr><td rowspan=1 colspan=1>v1.1</td><td rowspan=1 colspan=1>17-09-2025</td><td rowspan=1 colspan=1>Format lai file</td><td rowspan=1 colspan=1>P phucth12</td><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>□</td></tr></table>

# 1. Tổng quan

Hệ thống này định nghĩa các kiểu điều khiển kỹ năng (Skill Control Types) dành cho game mobile.

Mục tiêu:

• Chuẩn hóa input để dễ triển khai cho nhiều loại skill khác nhau. Hỗ trợ UX thân thiện, dễ thao tác trên màn hình cảm ứng.   
• Tối ưu sự đa dạng: từ skill buff nhanh, kỹ năng định hướng, kỹ năng chọn vị trí, đến kỹ năng duy trì (channeling).

# 2. Mục lục

3. Control Types

4. UI/UX

# 3. Control Types

<table><tr><td rowspan=1 colspan=1>ControlType</td><td rowspan=1 colspan=1>Dac diem</td><td rowspan=1 colspan=1>Use-case</td><td rowspan=1 colspan=1>Mobile Control</td><td rowspan=1 colspan=1>Auto Focus</td></tr><tr><td rowspan=1 colspan=1>Instant(Kich hoatngay）</td><td rowspan=1 colspan=1>Cast ngay khi bam Khóng bat buocphaitarget/huong/vi tride kich hoatPhan hoi nhanh</td><td rowspan=1 colspan=1>Buff ban than(giap,toc, hoimau) Trang thai (tanghinh,bat tu)</td><td rowspan=1 colspan=1>- Tap nut skill= kichhoat ngay- Khong can giu/kéotha</td><td rowspan=1 colspan=1></td></tr><tr><td rowspan=1 colspan=1>Direction(Ky nangtheohuong)</td><td rowspan=1 colspan=1> Yéu cau xac dinhhu6ng Skillshot c6 thé bine</td><td rowspan=1 colspan=1> Skillshot tam xa Dash dinh huongNé tranh</td><td rowspan=1 colspan=1> Manual: Nhän&amp; giu→hien vong dinh huóngKéo tay de chinhhuóng -&gt; Tha = cast theo huong chon</td><td rowspan=1 colspan=1>· C6 Target trongpham vi Kich hoat -&gt;Hien thinhanh dinh huongngay khi bam -&gt;Dinh huong Cast skilltheo vi tri cua Targetngay thoi diem kichhoat· Khóng c6 Target trong pham vi: Kich hoat -&gt; Cast skilltheo huong mäcdinh (truoc mat)theo pham vi toi dacua skill</td></tr><tr><td rowspan=1 colspan=1>Ground(Chon vitr1)</td><td rowspan=1 colspan=1> Yéu cäu chon 1diem tren ban doTao hieu ung dienrong (AOE)</td><td rowspan=1 colspan=1>DatbayGoi ky näng AOE(Meteor,Tornado)Zone Control</td><td rowspan=1 colspan=1>Manual: Nhän &amp; giu→hién vong chon khuvuc -&gt; Kéo tay de xäcdinh vi tri Tha = casttai vi tri chon</td><td rowspan=2 colspan=1>· C6 Target trongpham vi Auto cast skill taichan tuong dich bilock target</td></tr><tr><td rowspan=1 colspan=1></td><td></td><td></td><td></td></tr></table>

<table><tr><td rowspan=2 colspan=1></td><td rowspan=2 colspan=1></td><td rowspan=2 colspan=2></td><td rowspan=2 colspan=1>Auto Focus: Nhän -&gt;Hien thi nhanh dinhhuong theo AutoFocus -&gt; Kich hoat</td><td rowspan=1 colspan=1> Khong c6 Target·trong pham viNéu khong locktarget → cast vao</td></tr><tr><td rowspan=1 colspan=1>khoang cäch toi datheo huong joystick.</td></tr><tr><td rowspan=2 colspan=1>Channeling (Duytri)</td><td rowspan=2 colspan=1>Cast ngay lap tuc,duy tri den khi hétthdi gianC6 thé ngat boi Cchoäc khi thä nutMt so skill bätbu@c giu den hetchannel</td><td rowspan=2 colspan=2>Beam (tia laserduytri)Ban lien tuc Hoi mau lien tuc</td><td rowspan=1 colspan=1> Nhän &amp; giu = cast lién</td><td rowspan=1 colspan=1>Néu khong có：</td></tr><tr><td rowspan=1 colspan=1></td><td rowspan=1 colspan=1>tucNhän lai= cancelC6 thé chinh huóngbang joystickC6 thé bi cancel khi bi khöng ché (define tuyskill)</td><td rowspan=1 colspan=1>TargetBeam skill → bäntheo huong macdinh (truoc mät) taikhoang cach toi da.AOE Skill: dät vungchannel ngay tru8cmät caster theo pham vi preset(config 70% Range)cua Skill</td></tr><tr><td rowspan=1 colspan=1>CancelSkill</td><td rowspan=1 colspan=5>·Khi dang Hold skill sé hién thi button Cancel &amp; góc phai trén cung cua man hinh，Cancel bäng cách kéo joystick di chuyén lén nut cancel (X).</td></tr></table>

# 4. UI/UX

<table><tr><td colspan="1" rowspan="1">Control Type</td><td colspan="1" rowspan="1">UI</td><td colspan="1" rowspan="1">UX Behavior</td><td colspan="1" rowspan="1">Reference</td></tr><tr><td colspan="1" rowspan="1">Instant (Tapcast)</td><td colspan="1" rowspan="1">：  Icon skill säng lén khisan sang.</td><td colspan="1" rowspan="1">- Tap = cast ngay, khöng cänkéo.- Däm bao latency = 0.- Nguoichoi phai cäm thayresponsive.</td><td colspan="1" rowspan="1"></td></tr><tr><td colspan="1" rowspan="1">Direction(Skillshot)</td><td colspan="1" rowspan="1">：   Hien thi pham vi toi dacast cua skill theo hinhtron,bän kinh theo dódai toi da cua skill (1mau mäc dinh)·  Hien thi Duong Cast cuaskill (1 mau mäc dinh)</td><td colspan="1" rowspan="1">- Giu = hien huóng.- Kéo = thay doi huongrealtime.- Tha = cast theo huong.- Auto Aim Option: néu chitap → auto lock target gännhat.</td><td colspan="1" rowspan="1"></td></tr><tr><td></td><td>Khi kéo giu, icon skill doi . trang thai (expand).</td><td></td><td></td></tr><tr><td>Ground (AOE select)</td><td>Hien thivong tron bān · kinh AOE tai vi tri chi dinh Mau säc feedback: xanh · (valid),do (invalid). Highlight outline khu . vuc sé anh huong.</td><td>- Giu = hien vong AOE. - Kéo = thay doi vi tri. - Tha = cast. - Auto Mode: tap nhanh →&gt; cast auto duoi chän target hoäc theo huong joystick.</td><td></td></tr><tr><td>Channeling (Hold skill)</td><td>Hien thi pham vi cast theo hinh tron,bän kinh theo pham vi toi da cua skill Beam/laser→vé duong · thäng realtime</td><td>- Giu = duy tri cast. - Tha = cancel.</td><td></td></tr><tr><td>Cancel</td><td>Döi mau hien thi pham vi cast va mau cua dinh huöng skill</td><td></td><td></td></tr></table>