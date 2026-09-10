using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Text;

namespace EA_DB_Editor.Scheduling
{
    public static class TelevisionScheduler
    {
        private const string TvAllPlaceholder = "{tvall}";
        private const string ScheduleHTML = "PCFkb2N0eXBlIGh0bWw+CjxodG1sIGxhbmc9ImVuIj4KPGhlYWQ+CjxtZXRhIGNoYXJzZXQ9InV0Zi04Ij4KPG1ldGEgbmFtZT0idmlld3BvcnQiIGNvbnRlbnQ9IndpZHRoPWRldmljZS13aWR0aCwgaW5pdGlhbC1zY2FsZT0xIj4KPHRpdGxlPkNvbGxlZ2UgRm9vdGJhbGwgVFYgU2NoZWR1bGU8L3RpdGxlPgo8c3R5bGU+CiAgOnJvb3R7CiAgICAtLWdyZWVuOiMwZDdmNDE7CiAgICAtLWdyZWVuLWRhcms6IzBhNjUzNTsKICAgIC0tYmFyLWRhcms6IzFhMWExYTsKICAgIC0tYmFyLWRhcmsyOiMyNDI0MjQ7CiAgICAtLXN0cmlwZTojZjJmMmYyOwogICAgLS1ib3JkZXI6I2UwZTBlMDsKICAgIC0tdGV4dDojMWExYTFhOwogICAgLS10ZXh0LW11dGVkOiM2NjY7CiAgICAtLWxpbms6IzBkN2Y0MTsKICAgIC0tYmc6I2ZmZmZmZjsKICAgIC0tcGFnZS1iZzojZjVmNWY1OwogICAgLS10b2RheTojZmZmOGUxOwogICAgLS10b2RheS1ib3JkZXI6I2YwYjQyOTsKICB9CiAgKntib3gtc2l6aW5nOmJvcmRlci1ib3g7fQogIGh0bWwsYm9keXttYXJnaW46MDtwYWRkaW5nOjA7fQogIGJvZHl7CiAgICBiYWNrZ3JvdW5kOnZhcigtLXBhZ2UtYmcpOwogICAgY29sb3I6dmFyKC0tdGV4dCk7CiAgICBmb250LWZhbWlseTpIZWx2ZXRpY2EsQXJpYWwsc2Fucy1zZXJpZjsKICAgIGZvbnQtc2l6ZToxNHB4OwogICAgbGluZS1oZWlnaHQ6MS40OwogIH0KICBhe2NvbG9yOnZhcigtLWxpbmspO30KICAudG9wYmFyewogICAgYmFja2dyb3VuZDp2YXIoLS1iYXItZGFyayk7CiAgICBjb2xvcjojZmZmOwogICAgcGFkZGluZzoxNHB4IDIwcHg7CiAgICBkaXNwbGF5OmZsZXg7CiAgICBhbGlnbi1pdGVtczpjZW50ZXI7CiAgICBnYXA6MTBweDsKICAgIGZsZXgtd3JhcDp3cmFwOwogIH0KICAudG9wYmFyIC5sb2dvewogICAgZm9udC13ZWlnaHQ6ODAwOwogICAgZm9udC1zaXplOjIwcHg7CiAgICBsZXR0ZXItc3BhY2luZzouM3B4OwogICAgZGlzcGxheTpmbGV4OwogICAgYWxpZ24taXRlbXM6Y2VudGVyOwogICAgZ2FwOjhweDsKICB9CiAgLnRvcGJhciAubG9nbyAuYmFsbHtmb250LXNpemU6MjBweDt9CiAgLnRvcGJhciAuc2Vhc29uewogICAgbWFyZ2luLWxlZnQ6YXV0bzsKICAgIGJhY2tncm91bmQ6dmFyKC0tZ3JlZW4pOwogICAgY29sb3I6I2ZmZjsKICAgIGZvbnQtd2VpZ2h0OjcwMDsKICAgIGZvbnQtc2l6ZToxMnB4OwogICAgcGFkZGluZzo0cHggMTBweDsKICAgIGJvcmRlci1yYWRpdXM6MTJweDsKICAgIGxldHRlci1zcGFjaW5nOi41cHg7CiAgfQogIC53cmFwewogICAgbWF4LXdpZHRoOjkwMHB4OwogICAgbWFyZ2luOjAgYXV0bzsKICAgIHBhZGRpbmc6MjBweCAxNnB4IDYwcHg7CiAgfQogIGgxewogICAgZm9udC1zaXplOjIycHg7CiAgICBtYXJnaW46NHB4IDAgOHB4OwogIH0KICAuaW50cm97CiAgICBjb2xvcjp2YXIoLS10ZXh0LW11dGVkKTsKICAgIGZvbnQtc2l6ZToxM3B4OwogICAgbWFyZ2luLWJvdHRvbToxOHB4OwogIH0KICAuY29udHJvbHN7CiAgICBiYWNrZ3JvdW5kOnZhcigtLWJnKTsKICAgIGJvcmRlcjoxcHggc29saWQgdmFyKC0tYm9yZGVyKTsKICAgIGJvcmRlci1yYWRpdXM6NnB4OwogICAgcGFkZGluZzoxNHB4OwogICAgbWFyZ2luLWJvdHRvbToxOHB4OwogIH0KICAud2Vla3Jvd3sKICAgIGRpc3BsYXk6ZmxleDsKICAgIGZsZXgtd3JhcDp3cmFwOwogICAgZ2FwOjZweDsKICAgIG1hcmdpbi1ib3R0b206MTJweDsKICB9CiAgLndlZWstYnRuewogICAgYm9yZGVyOjFweCBzb2xpZCB2YXIoLS1ib3JkZXIpOwogICAgYmFja2dyb3VuZDojZmFmYWZhOwogICAgY29sb3I6dmFyKC0tdGV4dCk7CiAgICBmb250LXNpemU6MTJweDsKICAgIGZvbnQtd2VpZ2h0OjYwMDsKICAgIHBhZGRpbmc6NnB4IDExcHg7CiAgICBib3JkZXItcmFkaXVzOjE0cHg7CiAgICBjdXJzb3I6cG9pbnRlcjsKICB9CiAgLndlZWstYnRuOmhvdmVye2JhY2tncm91bmQ6I2VlZTt9CiAgLndlZWstYnRuLmFjdGl2ZXsKICAgIGJhY2tncm91bmQ6dmFyKC0tZ3JlZW4pOwogICAgYm9yZGVyLWNvbG9yOnZhcigtLWdyZWVuKTsKICAgIGNvbG9yOiNmZmY7CiAgfQogIC5maWx0ZXJzewogICAgZGlzcGxheTpmbGV4OwogICAgZmxleC13cmFwOndyYXA7CiAgICBnYXA6MTBweDsKICAgIGFsaWduLWl0ZW1zOmNlbnRlcjsKICB9CiAgLmZpbHRlcnMgaW5wdXRbdHlwZT0ic2VhcmNoIl17CiAgICBmbGV4OjE7CiAgICBtaW4td2lkdGg6MTgwcHg7CiAgICBwYWRkaW5nOjhweCAxMHB4OwogICAgYm9yZGVyOjFweCBzb2xpZCB2YXIoLS1ib3JkZXIpOwogICAgYm9yZGVyLXJhZGl1czo0cHg7CiAgICBmb250LXNpemU6MTNweDsKICB9CiAgLmZpbHRlcnMgc2VsZWN0ewogICAgcGFkZGluZzo4cHggMTBweDsKICAgIGJvcmRlcjoxcHggc29saWQgdmFyKC0tYm9yZGVyKTsKICAgIGJvcmRlci1yYWRpdXM6NHB4OwogICAgZm9udC1zaXplOjEzcHg7CiAgICBiYWNrZ3JvdW5kOiNmZmY7CiAgfQogIC5maWx0ZXJzIGJ1dHRvbi50b2RheS1idG57CiAgICBwYWRkaW5nOjhweCAxMnB4OwogICAgYm9yZGVyOjFweCBzb2xpZCB2YXIoLS1ncmVlbik7CiAgICBiYWNrZ3JvdW5kOiNmZmY7CiAgICBjb2xvcjp2YXIoLS1ncmVlbik7CiAgICBib3JkZXItcmFkaXVzOjRweDsKICAgIGZvbnQtc2l6ZToxM3B4OwogICAgZm9udC13ZWlnaHQ6NzAwOwogICAgY3Vyc29yOnBvaW50ZXI7CiAgfQogIC5maWx0ZXJzIGJ1dHRvbi50b2RheS1idG46aG92ZXJ7YmFja2dyb3VuZDp2YXIoLS1ncmVlbik7Y29sb3I6I2ZmZjt9CgogIC5zY2hlZHVsZS10aXRsZXsKICAgIGJhY2tncm91bmQ6dmFyKC0tYmFyLWRhcmsyKTsKICAgIGNvbG9yOiNmZmY7CiAgICBmb250LXdlaWdodDo3MDA7CiAgICBmb250LXNpemU6MTRweDsKICAgIGxldHRlci1zcGFjaW5nOi40cHg7CiAgICBwYWRkaW5nOjEwcHggMTRweDsKICAgIG1hcmdpbi10b3A6MjJweDsKICAgIGJvcmRlci1yYWRpdXM6NHB4IDRweCAwIDA7CiAgfQogIC5kYXktYmxvY2t7Ym9yZGVyOjFweCBzb2xpZCB2YXIoLS1ib3JkZXIpO2JvcmRlci10b3A6bm9uZTt9CiAgLmRheS1oZWFkZXJ7CiAgICBiYWNrZ3JvdW5kOnZhcigtLWdyZWVuKTsKICAgIGNvbG9yOiNmZmY7CiAgICBmb250LXdlaWdodDo3MDA7CiAgICBmb250LXNpemU6MTNweDsKICAgIHBhZGRpbmc6OHB4IDE0cHg7CiAgfQogIC5kYXktaGVhZGVyLnRvZGF5ewogICAgYmFja2dyb3VuZDp2YXIoLS10b2RheS1ib3JkZXIpOwogICAgY29sb3I6IzNhMmEwMDsKICB9CiAgdGFibGUuZ2FtZXN7CiAgICB3aWR0aDoxMDAlOwogICAgYm9yZGVyLWNvbGxhcHNlOmNvbGxhcHNlOwogIH0KICB0YWJsZS5nYW1lcyB0aHsKICAgIGJhY2tncm91bmQ6I2U5ZTllOTsKICAgIHRleHQtYWxpZ246bGVmdDsKICAgIGZvbnQtc2l6ZToxMXB4OwogICAgdGV4dC10cmFuc2Zvcm06dXBwZXJjYXNlOwogICAgbGV0dGVyLXNwYWNpbmc6LjRweDsKICAgIGNvbG9yOnZhcigtLXRleHQtbXV0ZWQpOwogICAgcGFkZGluZzo3cHggMTRweDsKICAgIGZvbnQtd2VpZ2h0OjcwMDsKICB9CiAgdGFibGUuZ2FtZXMgdGR7CiAgICBwYWRkaW5nOjlweCAxNHB4OwogICAgYm9yZGVyLXRvcDoxcHggc29saWQgdmFyKC0tYm9yZGVyKTsKICAgIHZlcnRpY2FsLWFsaWduOm1pZGRsZTsKICAgIGZvbnQtc2l6ZToxM3B4OwogIH0KICB0YWJsZS5nYW1lcyB0ci5vZGQgdGR7YmFja2dyb3VuZDp2YXIoLS1zdHJpcGUpO30KICB0YWJsZS5nYW1lcyB0cjpob3ZlciB0ZHtiYWNrZ3JvdW5kOiNlOGYzZWM7fQogIHRhYmxlLmdhbWVzIHRyLnRvZGF5LXJvdyB0ZHtiYWNrZ3JvdW5kOnZhcigtLXRvZGF5KTt9CiAgLm1hdGNodXB7d2hpdGUtc3BhY2U6bm93cmFwO30KICAubWF0Y2h1cCAucmFua3sKICAgIGNvbG9yOnZhcigtLXRleHQtbXV0ZWQpOwogICAgZm9udC1zaXplOjExcHg7CiAgICBmb250LXdlaWdodDo3MDA7CiAgICBtYXJnaW4tcmlnaHQ6MnB4OwogIH0KICAubWF0Y2h1cCAuYXR7CiAgICBjb2xvcjp2YXIoLS10ZXh0LW11dGVkKTsKICAgIGZvbnQtc2l6ZToxMXB4OwogICAgbWFyZ2luOjAgNnB4OwogICAgdGV4dC10cmFuc2Zvcm06dXBwZXJjYXNlOwogIH0KICAubWF0Y2h1cCAudGVhbS5ob21le2ZvbnQtd2VpZ2h0OjcwMDt9CiAgLnRpbWV7d2hpdGUtc3BhY2U6bm93cmFwO2NvbG9yOnZhcigtLXRleHQtbXV0ZWQpO2ZvbnQtd2VpZ2h0OjYwMDt9CiAgLmNoYW57CiAgICBkaXNwbGF5OmlubGluZS1ibG9jazsKICAgIGJhY2tncm91bmQ6I2VlZjRmMDsKICAgIGJvcmRlcjoxcHggc29saWQgI2Q3ZTZkYzsKICAgIGNvbG9yOnZhcigtLWdyZWVuLWRhcmspOwogICAgZm9udC13ZWlnaHQ6NzAwOwogICAgZm9udC1zaXplOjExcHg7CiAgICBwYWRkaW5nOjNweCA4cHg7CiAgICBib3JkZXItcmFkaXVzOjNweDsKICAgIHdoaXRlLXNwYWNlOm5vd3JhcDsKICB9CiAgLmVtcHR5ewogICAgcGFkZGluZzozMHB4OwogICAgdGV4dC1hbGlnbjpjZW50ZXI7CiAgICBjb2xvcjp2YXIoLS10ZXh0LW11dGVkKTsKICAgIGJhY2tncm91bmQ6I2ZmZjsKICAgIGJvcmRlcjoxcHggc29saWQgdmFyKC0tYm9yZGVyKTsKICAgIGJvcmRlci10b3A6bm9uZTsKICB9CiAgLndlZWstc2VhcmNoLWxhYmVsewogICAgYmFja2dyb3VuZDojMzMzOwogICAgY29sb3I6I2ZmZjsKICAgIGZvbnQtc2l6ZToxMnB4OwogICAgZm9udC13ZWlnaHQ6NzAwOwogICAgcGFkZGluZzo4cHggMTRweDsKICAgIG1hcmdpbi10b3A6MjJweDsKICAgIGJvcmRlci1yYWRpdXM6NHB4IDRweCAwIDA7CiAgfQogIGZvb3RlcnsKICAgIHRleHQtYWxpZ246Y2VudGVyOwogICAgY29sb3I6dmFyKC0tdGV4dC1tdXRlZCk7CiAgICBmb250LXNpemU6MTFweDsKICAgIG1hcmdpbi10b3A6MzBweDsKICB9CiAgQG1lZGlhIChtYXgtd2lkdGg6NTIwcHgpewogICAgdGFibGUuZ2FtZXMgdGg6bnRoLWNoaWxkKDMpLCB0YWJsZS5nYW1lcyB0ZDpudGgtY2hpbGQoMyl7ZGlzcGxheTpub25lO30KICAgIC5tYXRjaHVwe3doaXRlLXNwYWNlOm5vcm1hbDt9CiAgfQo8L3N0eWxlPgo8L2hlYWQ+Cjxib2R5PgoKPGRpdiBjbGFzcz0idG9wYmFyIj4KICA8ZGl2IGNsYXNzPSJsb2dvIj48c3BhbiBjbGFzcz0iYmFsbCI+8J+PiDwvc3Bhbj4gQ29sbGVnZSBGb290YmFsbCBUViBTY2hlZHVsZTwvZGl2PgogIDxkaXYgY2xhc3M9InNlYXNvbiIgaWQ9InNlYXNvbkJhZGdlIj5TRUFTT048L2Rpdj4KPC9kaXY+Cgo8ZGl2IGNsYXNzPSJ3cmFwIj4KICA8aDE+Q29sbGVnZSBGb290YmFsbCBUViBTY2hlZHVsZTwvaDE+CiAgPGRpdiBjbGFzcz0iaW50cm8iPkV2ZXJ5IGdhbWUgYmVsb3csIHdpdGgga2lja29mZiB0aW1lIGFuZCBUViBjaGFubmVsLiBQaWNrIGEgd2Vlaywgc2VhcmNoIGEgdGVhbSwgb3IganVtcCB0byB0b2RheSdzIGdhbWVzLjwvZGl2PgoKICA8ZGl2IGNsYXNzPSJjb250cm9scyI+CiAgICA8ZGl2IGNsYXNzPSJ3ZWVrcm93IiBpZD0id2Vla1JvdyI+PC9kaXY+CiAgICA8ZGl2IGNsYXNzPSJmaWx0ZXJzIj4KICAgICAgPGlucHV0IHR5cGU9InNlYXJjaCIgaWQ9InRlYW1TZWFyY2giIHBsYWNlaG9sZGVyPSJTZWFyY2ggYnkgdGVhbSBuYW1l4oCmIj4KICAgICAgPHNlbGVjdCBpZD0iY2hhbm5lbEZpbHRlciI+PG9wdGlvbiB2YWx1ZT0iIj5BbGwgY2hhbm5lbHM8L29wdGlvbj48L3NlbGVjdD4KICAgICAgPHNlbGVjdCBpZD0iY29uZmVyZW5jZUZpbHRlciI+PG9wdGlvbiB2YWx1ZT0iIj5BbGwgY29uZmVyZW5jZXM8L29wdGlvbj48L3NlbGVjdD4KICAgICAgPGJ1dHRvbiBjbGFzcz0idG9kYXktYnRuIiBpZD0idG9kYXlCdG4iPkp1bXAgdG8gVG9kYXk8L2J1dHRvbj4KICAgIDwvZGl2PgogIDwvZGl2PgoKICA8ZGl2IGlkPSJzY2hlZHVsZU91dHB1dCI+PC9kaXY+CgogIDxmb290ZXI+R2VuZXJhdGVkIGZyb20gdHYtYWxsLnR4dCAmbWlkZG90OyA4MTEgZ2FtZXM8L2Zvb3Rlcj4KPC9kaXY+Cgo8c2NyaXB0IGlkPSJnYW1lRGF0YSIgdHlwZT0iYXBwbGljYXRpb24vanNvbiI+e3R2YWxsfTwvc2NyaXB0Pgo8c2NyaXB0PgooZnVuY3Rpb24oKXsKICBjb25zdCBfc3JjID0gSlNPTi5wYXJzZShkb2N1bWVudC5nZXRFbGVtZW50QnlJZCgnZ2FtZURhdGEnKS50ZXh0Q29udGVudCk7CiAgY29uc3QgYWxsR2FtZXMgPSBfc3JjLmFsbEdhbWVzOwogIC8vIFNvdXJjZSBkYXRhIGlzIGJ1aWx0IGZyb20gYSBmaXhlZCB3ZWVrbHkgdGVtcGxhdGUgKGUuZy4gIldlZWsgTiBpcyBhbHdheXMKICAvLyBBdWd1c3QgMzEiKTsgdGhlIHJlYWwtd29ybGQgd2Vla2RheSB0aGF0IGRhdGUgZmFsbHMgb24gZGVwZW5kcyBvbiB0aGUgc2Vhc29uCiAgLy8geWVhciBhbmQgaXMgaXJyZWxldmFudCBoZXJlLCBzbyBkYXkgbmFtZXMgYXJlIHRha2VuIGZyb20gZWFjaCBnYW1lJ3Mgb3duCiAgLy8gIkRheSIgZmllbGQgcmF0aGVyIHRoYW4gcmVjb21wdXRlZCBmcm9tIHRoZSBjYWxlbmRhci4KICBjb25zdCBTRUFTT05fWUVBUiA9IF9zcmMueWVhciB8fCBuZXcgRGF0ZSgpLmdldEZ1bGxZZWFyKCk7CgogIGNvbnN0IE1PTlRIX0lEWCA9IHtKYW51YXJ5OjAsRmVicnVhcnk6MSxNYXJjaDoyLEFwcmlsOjMsTWF5OjQsSnVuZTo1LEp1bHk6NixBdWd1c3Q6NyxTZXB0ZW1iZXI6OCxPY3RvYmVyOjksTm92ZW1iZXI6MTAsRGVjZW1iZXI6MTF9OwogIGNvbnN0IE1PTlRIX05BTUVTID0gWydKYW51YXJ5JywnRmVicnVhcnknLCdNYXJjaCcsJ0FwcmlsJywnTWF5JywnSnVuZScsJ0p1bHknLCdBdWd1c3QnLCdTZXB0ZW1iZXInLCdPY3RvYmVyJywnTm92ZW1iZXInLCdEZWNlbWJlciddOwogIC8vIE1vbnRoL0RhdGUgaXMgZWFjaCBnYW1lJ3Mgb3duIGFjY3VyYXRlIGRheS1vZi1tb250aDsgRGF5IGlzIGp1c3QgaXRzIHdlZWtkYXkKICAvLyBuYW1lIGZvciBkaXNwbGF5LiBObyBvZmZzZXQgbWF0aCBuZWVkZWQgLS0gdXNlIHRoZW0gYXMgZ2l2ZW4uCgogIGRvY3VtZW50LmdldEVsZW1lbnRCeUlkKCdzZWFzb25CYWRnZScpLnRleHRDb250ZW50ID0gU0VBU09OX1lFQVIgKyAnIFNFQVNPTic7CgogIGFsbEdhbWVzLmZvckVhY2goZz0+ewogICAgZy5fZGF0ZSA9IG5ldyBEYXRlKFNFQVNPTl9ZRUFSLCBNT05USF9JRFhbZy5Nb250aF0sIGcuRGF0ZSk7CiAgICBjb25zdCBtID0gLyhcZHsxLDJ9KTooXGR7Mn0pKEFNfFBNKS8uZXhlYyhnLlRpbWUpOwogICAgbGV0IGhoID0gcGFyc2VJbnQobVsxXSwxMCksIG1tID0gcGFyc2VJbnQobVsyXSwxMCk7CiAgICBpZihtWzNdPT09J1BNJyAmJiBoaCE9PTEyKSBoaCs9MTI7CiAgICBpZihtWzNdPT09J0FNJyAmJiBoaD09PTEyKSBoaD0wOwogICAgZy5fbWludXRlcyA9IGhoKjYwK21tOwogIH0pOwoKICBjb25zdCB3ZWVrcyA9IFsuLi5uZXcgU2V0KGFsbEdhbWVzLm1hcChnPT5nLldlZWspKV0uc29ydCgoYSxiKT0+YS1iKTsKICBjb25zdCBjaGFubmVscyA9IFsuLi5uZXcgU2V0KGFsbEdhbWVzLm1hcChnPT5nLkNoYW5uZWwpKV0uc29ydCgpOwogIGNvbnN0IGNvbmZlcmVuY2VzID0gWy4uLm5ldyBTZXQoYWxsR2FtZXMuZmxhdE1hcChnPT5bZy5Ib21lQ29uZmVyZW5jZSwgZy5Bd2F5Q29uZmVyZW5jZV0pKV0uZmlsdGVyKEJvb2xlYW4pLnNvcnQoKTsKCiAgY29uc3QgdG9kYXkgPSBuZXcgRGF0ZSgpOwogIHRvZGF5LnNldEhvdXJzKDAsMCwwLDApOwogIGZ1bmN0aW9uIHNhbWVEYXkoYSxiKXsgcmV0dXJuIGEuZ2V0RnVsbFllYXIoKT09PWIuZ2V0RnVsbFllYXIoKSAmJiBhLmdldE1vbnRoKCk9PT1iLmdldE1vbnRoKCkgJiYgYS5nZXREYXRlKCk9PT1iLmdldERhdGUoKTsgfQoKICAvLyBwaWNrIGRlZmF1bHQgd2VlazogdGhlIG9uZSB3aG9zZSBnYW1lcyBhcmUgY2xvc2VzdCB0byAoY292ZXJpbmcpIHRvZGF5CiAgZnVuY3Rpb24gZGVmYXVsdFdlZWsoKXsKICAgIGxldCBiZXN0ID0gd2Vla3NbMF0sIGJlc3REaWZmID0gSW5maW5pdHk7CiAgICBmb3IoY29uc3QgdyBvZiB3ZWVrcyl7CiAgICAgIGNvbnN0IHdnID0gYWxsR2FtZXMuZmlsdGVyKGc9PmcuV2Vlaz09PXcpOwogICAgICBjb25zdCBtaW4gPSB3Zy5yZWR1Y2UoKGEsZyk9PmcuX2RhdGU8YT9nLl9kYXRlOmEsIHdnWzBdLl9kYXRlKTsKICAgICAgY29uc3QgbWF4ID0gd2cucmVkdWNlKChhLGcpPT5nLl9kYXRlPmE/Zy5fZGF0ZTphLCB3Z1swXS5fZGF0ZSk7CiAgICAgIGlmKHRvZGF5Pj1taW4gJiYgdG9kYXk8PW1heCkgcmV0dXJuIHc7CiAgICAgIGNvbnN0IGRpZmYgPSB0b2RheTxtaW4gPyAobWluLXRvZGF5KSA6ICh0b2RheS1tYXgpOwogICAgICBpZihkaWZmIDwgYmVzdERpZmYpeyBiZXN0RGlmZiA9IGRpZmY7IGJlc3QgPSB3OyB9CiAgICB9CiAgICByZXR1cm4gYmVzdDsKICB9CgogIGxldCBjdXJyZW50V2VlayA9IGRlZmF1bHRXZWVrKCk7CiAgbGV0IHNlYXJjaFRlcm0gPSAnJzsKICBsZXQgY2hhbm5lbEZpbHRlciA9ICcnOwogIGxldCBjb25mZXJlbmNlRmlsdGVyID0gJyc7CgogIGNvbnN0IHdlZWtSb3cgPSBkb2N1bWVudC5nZXRFbGVtZW50QnlJZCgnd2Vla1JvdycpOwogIHdlZWtzLmZvckVhY2godz0+ewogICAgY29uc3QgYiA9IGRvY3VtZW50LmNyZWF0ZUVsZW1lbnQoJ2J1dHRvbicpOwogICAgYi5jbGFzc05hbWUgPSAnd2Vlay1idG4nOwogICAgYi50ZXh0Q29udGVudCA9IHc9PT0wID8gJ1dlZWsgMCcgOiAnV2VlayAnK3c7CiAgICBiLmRhdGFzZXQud2VlayA9IHc7CiAgICBiLmFkZEV2ZW50TGlzdGVuZXIoJ2NsaWNrJywgKCk9PnsgY3VycmVudFdlZWsgPSB3OyByZW5kZXIoKTsgfSk7CiAgICB3ZWVrUm93LmFwcGVuZENoaWxkKGIpOwogIH0pOwoKICBjb25zdCBjaGFuU2VsID0gZG9jdW1lbnQuZ2V0RWxlbWVudEJ5SWQoJ2NoYW5uZWxGaWx0ZXInKTsKICBjaGFubmVscy5mb3JFYWNoKGM9PnsKICAgIGNvbnN0IG8gPSBkb2N1bWVudC5jcmVhdGVFbGVtZW50KCdvcHRpb24nKTsKICAgIG8udmFsdWUgPSBjOyBvLnRleHRDb250ZW50ID0gYzsKICAgIGNoYW5TZWwuYXBwZW5kQ2hpbGQobyk7CiAgfSk7CiAgY2hhblNlbC5hZGRFdmVudExpc3RlbmVyKCdjaGFuZ2UnLCAoKT0+eyBjaGFubmVsRmlsdGVyID0gY2hhblNlbC52YWx1ZTsgcmVuZGVyKCk7IH0pOwoKICBjb25zdCBjb25mU2VsID0gZG9jdW1lbnQuZ2V0RWxlbWVudEJ5SWQoJ2NvbmZlcmVuY2VGaWx0ZXInKTsKICBjb25mZXJlbmNlcy5mb3JFYWNoKGM9PnsKICAgIGNvbnN0IG8gPSBkb2N1bWVudC5jcmVhdGVFbGVtZW50KCdvcHRpb24nKTsKICAgIG8udmFsdWUgPSBjOyBvLnRleHRDb250ZW50ID0gYzsKICAgIGNvbmZTZWwuYXBwZW5kQ2hpbGQobyk7CiAgfSk7CiAgY29uZlNlbC5hZGRFdmVudExpc3RlbmVyKCdjaGFuZ2UnLCAoKT0+eyBjb25mZXJlbmNlRmlsdGVyID0gY29uZlNlbC52YWx1ZTsgcmVuZGVyKCk7IH0pOwoKICBkb2N1bWVudC5nZXRFbGVtZW50QnlJZCgndGVhbVNlYXJjaCcpLmFkZEV2ZW50TGlzdGVuZXIoJ2lucHV0JywgKGUpPT57CiAgICBzZWFyY2hUZXJtID0gZS50YXJnZXQudmFsdWUudHJpbSgpLnRvTG93ZXJDYXNlKCk7CiAgICByZW5kZXIoKTsKICB9KTsKCiAgZG9jdW1lbnQuZ2V0RWxlbWVudEJ5SWQoJ3RvZGF5QnRuJykuYWRkRXZlbnRMaXN0ZW5lcignY2xpY2snLCAoKT0+ewogICAgY3VycmVudFdlZWsgPSBkZWZhdWx0V2VlaygpOwogICAgZG9jdW1lbnQuZ2V0RWxlbWVudEJ5SWQoJ3RlYW1TZWFyY2gnKS52YWx1ZSA9ICcnOwogICAgc2VhcmNoVGVybSA9ICcnOwogICAgcmVuZGVyKCk7CiAgICBzZXRUaW1lb3V0KCgpPT57CiAgICAgIGNvbnN0IGVsID0gZG9jdW1lbnQucXVlcnlTZWxlY3RvcignLnRvZGF5LXJvdywgLmRheS1oZWFkZXIudG9kYXknKTsKICAgICAgaWYoZWwpIGVsLnNjcm9sbEludG9WaWV3KHtiZWhhdmlvcjonc21vb3RoJywgYmxvY2s6J2NlbnRlcid9KTsKICAgIH0sIDMwKTsKICB9KTsKCiAgZnVuY3Rpb24gZm10RGF0ZShnKXsgcmV0dXJuIGcuRGF5ICsgJywgJyArIE1PTlRIX05BTUVTW2cuX2RhdGUuZ2V0TW9udGgoKV0gKyAnICcgKyBnLl9kYXRlLmdldERhdGUoKTsgfQogIGZ1bmN0aW9uIGZtdFRpbWUodCl7IHJldHVybiB0LnJlcGxhY2UoJ0FNJywnIEFNJykucmVwbGFjZSgnUE0nLCcgUE0nKS50b0xvd2VyQ2FzZSgpOyB9CgogIGZ1bmN0aW9uIHJhbmtTcGFuKHJhbmssIG5hbWUsIGlzSG9tZSl7CiAgICBjb25zdCBjbHMgPSAndGVhbScgKyAoaXNIb21lID8gJyBob21lJyA6ICcnKTsKICAgIGlmKHJhbmsgJiYgcmFuazw9MjUpewogICAgICByZXR1cm4gJzxzcGFuIGNsYXNzPSJyYW5rIj4jJytyYW5rKyc8L3NwYW4+PHNwYW4gY2xhc3M9IicrY2xzKyciPicrZXNjKG5hbWUpKyc8L3NwYW4+JzsKICAgIH0KICAgIHJldHVybiAnPHNwYW4gY2xhc3M9IicrY2xzKyciPicrZXNjKG5hbWUpKyc8L3NwYW4+JzsKICB9CgogIGZ1bmN0aW9uIGVzYyhzKXsKICAgIHJldHVybiBTdHJpbmcocykucmVwbGFjZSgvWyY8PiInXS9nLCBjPT4oeycmJzonJmFtcDsnLCc8JzonJmx0OycsJz4nOicmZ3Q7JywnIic6JyZxdW90OycsIiciOicmIzM5Oyd9W2NdKSk7CiAgfQoKICBmdW5jdGlvbiBidWlsZFRhYmxlKGdhbWVzSW4pewogICAgY29uc3QgZ2FtZXMgPSBbLi4uZ2FtZXNJbl0uc29ydCgoYSxiKT0+YS5fbWludXRlcy1iLl9taW51dGVzKTsKICAgIGxldCByb3dzID0gJyc7CiAgICBnYW1lcy5mb3JFYWNoKChnLGkpPT57CiAgICAgIGNvbnN0IGlzVG9kYXkgPSBzYW1lRGF5KGcuX2RhdGUsIHRvZGF5KTsKICAgICAgY29uc3QgdHJDbHMgPSAoaXNUb2RheSA/ICd0b2RheS1yb3cgJyA6ICcnKSArIChpJTI9PT0xID8gJ29kZCcgOiAnJyk7CiAgICAgIHJvd3MgKz0gJzx0ciBjbGFzcz0iJyt0ckNscy50cmltKCkrJyI+JwogICAgICAgICsgJzx0ZCBjbGFzcz0ibWF0Y2h1cCI+JytyYW5rU3BhbihnLkF3YXlSYW5rLCBnLkF3YXksIGZhbHNlKSsnPHNwYW4gY2xhc3M9ImF0Ij5hdDwvc3Bhbj4nK3JhbmtTcGFuKGcuSG9tZVJhbmssIGcuSG9tZSwgdHJ1ZSkrJzwvdGQ+JwogICAgICAgICsgJzx0ZCBjbGFzcz0idGltZSI+JytmbXRUaW1lKGcuVGltZSkrJzwvdGQ+JwogICAgICAgICsgJzx0ZD48c3BhbiBjbGFzcz0iY2hhbiI+Jytlc2MoZy5DaGFubmVsKSsnPC9zcGFuPjwvdGQ+JwogICAgICAgICsgJzwvdHI+JzsKICAgIH0pOwogICAgcmV0dXJuICc8dGFibGUgY2xhc3M9ImdhbWVzIj48dHI+PHRoPk1hdGNodXA8L3RoPjx0aD5UaW1lIChFVCk8L3RoPjx0aD5UVjwvdGg+PC90cj4nK3Jvd3MrJzwvdGFibGU+JzsKICB9CgogIGZ1bmN0aW9uIGdyb3VwQnlEYXkoZ2FtZXMpewogICAgY29uc3QgbWFwID0gbmV3IE1hcCgpOwogICAgZ2FtZXMuZm9yRWFjaChnPT57CiAgICAgIGNvbnN0IGtleSA9IGcuX2RhdGUuZ2V0VGltZSgpOwogICAgICBpZighbWFwLmhhcyhrZXkpKSBtYXAuc2V0KGtleSwgW10pOwogICAgICBtYXAuZ2V0KGtleSkucHVzaChnKTsKICAgIH0pOwogICAgcmV0dXJuIFsuLi5tYXAuZW50cmllcygpXS5zb3J0KChhLGIpPT5hWzBdLWJbMF0pLm1hcChlPT5lWzFdKTsKICB9CgogIGZ1bmN0aW9uIGFwcGx5RmlsdGVycyhnYW1lcyl7CiAgICByZXR1cm4gZ2FtZXMuZmlsdGVyKGc9PnsKICAgICAgaWYoY2hhbm5lbEZpbHRlciAmJiBnLkNoYW5uZWwhPT1jaGFubmVsRmlsdGVyKSByZXR1cm4gZmFsc2U7CiAgICAgIGlmKGNvbmZlcmVuY2VGaWx0ZXIgJiYgZy5Ib21lQ29uZmVyZW5jZSE9PWNvbmZlcmVuY2VGaWx0ZXIgJiYgZy5Bd2F5Q29uZmVyZW5jZSE9PWNvbmZlcmVuY2VGaWx0ZXIpIHJldHVybiBmYWxzZTsKICAgICAgaWYoc2VhcmNoVGVybSl7CiAgICAgICAgY29uc3QgaGF5ID0gKGcuSG9tZSsnICcrZy5Bd2F5KS50b0xvd2VyQ2FzZSgpOwogICAgICAgIGlmKCFoYXkuaW5jbHVkZXMoc2VhcmNoVGVybSkpIHJldHVybiBmYWxzZTsKICAgICAgfQogICAgICByZXR1cm4gdHJ1ZTsKICAgIH0pOwogIH0KCiAgZnVuY3Rpb24gcmVuZGVyKCl7CiAgICBbLi4ud2Vla1Jvdy5jaGlsZHJlbl0uZm9yRWFjaChiPT5iLmNsYXNzTGlzdC50b2dnbGUoJ2FjdGl2ZScsIE51bWJlcihiLmRhdGFzZXQud2Vlayk9PT1jdXJyZW50V2VlaykpOwoKICAgIGNvbnN0IG91dCA9IGRvY3VtZW50LmdldEVsZW1lbnRCeUlkKCdzY2hlZHVsZU91dHB1dCcpOwogICAgb3V0LmlubmVySFRNTCA9ICcnOwoKICAgIGlmKHNlYXJjaFRlcm0pewogICAgICAvLyBnbG9iYWwgc2VhcmNoIGFjcm9zcyBhbGwgd2Vla3MKICAgICAgY29uc3QgbWF0Y2hlcyA9IGFwcGx5RmlsdGVycyhhbGxHYW1lcyk7CiAgICAgIGlmKG1hdGNoZXMubGVuZ3RoPT09MCl7CiAgICAgICAgb3V0LmlubmVySFRNTCA9ICc8ZGl2IGNsYXNzPSJ3ZWVrLXNlYXJjaC1sYWJlbCI+U0VBUkNIIFJFU1VMVFM8L2Rpdj48ZGl2IGNsYXNzPSJlbXB0eSI+Tm8gZ2FtZXMgbWF0Y2ggeW91ciBzZWFyY2guPC9kaXY+JzsKICAgICAgICByZXR1cm47CiAgICAgIH0KICAgICAgb3V0LmlubmVySFRNTCArPSAnPGRpdiBjbGFzcz0id2Vlay1zZWFyY2gtbGFiZWwiPlNFQVJDSCBSRVNVTFRTICZtZGFzaDsgJyttYXRjaGVzLmxlbmd0aCsnIGdhbWUnKyhtYXRjaGVzLmxlbmd0aD09PTE/Jyc6J3MnKSsnPC9kaXY+JzsKICAgICAgY29uc3QgZGF5R3JvdXBzID0gZ3JvdXBCeURheShtYXRjaGVzKTsKICAgICAgY29uc3QgYmxvY2sgPSBkb2N1bWVudC5jcmVhdGVFbGVtZW50KCdkaXYnKTsKICAgICAgYmxvY2suY2xhc3NOYW1lID0gJ2RheS1ibG9jayc7CiAgICAgIGRheUdyb3Vwcy5mb3JFYWNoKGRnPT57CiAgICAgICAgY29uc3QgaXNUb2RheUdyb3VwID0gc2FtZURheShkZ1swXS5fZGF0ZSwgdG9kYXkpOwogICAgICAgIGJsb2NrLmlubmVySFRNTCArPSAnPGRpdiBjbGFzcz0iZGF5LWhlYWRlcicrKGlzVG9kYXlHcm91cD8nIHRvZGF5JzonJykrJyI+V2VlayAnK2RnWzBdLldlZWsrJyAmbWlkZG90OyAnK2ZtdERhdGUoZGdbMF0pKyc8L2Rpdj4nICsgYnVpbGRUYWJsZShkZyk7CiAgICAgIH0pOwogICAgICBvdXQuYXBwZW5kQ2hpbGQoYmxvY2spOwogICAgICByZXR1cm47CiAgICB9CgogICAgY29uc3Qgd2Vla0dhbWVzID0gYXBwbHlGaWx0ZXJzKGFsbEdhbWVzLmZpbHRlcihnPT5nLldlZWs9PT1jdXJyZW50V2VlaykpOwogICAgb3V0LmlubmVySFRNTCArPSAnPGRpdiBjbGFzcz0ic2NoZWR1bGUtdGl0bGUiPkNPTExFR0UgRk9PVEJBTEwgU0NIRURVTEUgJm1kYXNoOyBXRUVLICcrY3VycmVudFdlZWsrJzwvZGl2Pic7CiAgICBpZih3ZWVrR2FtZXMubGVuZ3RoPT09MCl7CiAgICAgIG91dC5pbm5lckhUTUwgKz0gJzxkaXYgY2xhc3M9ImVtcHR5Ij5ObyBnYW1lcyBtYXRjaCB0aGUgc2VsZWN0ZWQgZmlsdGVycy48L2Rpdj4nOwogICAgICByZXR1cm47CiAgICB9CiAgICBjb25zdCBkYXlHcm91cHMgPSBncm91cEJ5RGF5KHdlZWtHYW1lcyk7CiAgICBjb25zdCBibG9jayA9IGRvY3VtZW50LmNyZWF0ZUVsZW1lbnQoJ2RpdicpOwogICAgYmxvY2suY2xhc3NOYW1lID0gJ2RheS1ibG9jayc7CiAgICBkYXlHcm91cHMuZm9yRWFjaChkZz0+ewogICAgICBjb25zdCBpc1RvZGF5R3JvdXAgPSBzYW1lRGF5KGRnWzBdLl9kYXRlLCB0b2RheSk7CiAgICAgIGJsb2NrLmlubmVySFRNTCArPSAnPGRpdiBjbGFzcz0iZGF5LWhlYWRlcicrKGlzVG9kYXlHcm91cD8nIHRvZGF5JzonJykrJyI+JytmbXREYXRlKGRnWzBdKSsnPC9kaXY+JyArIGJ1aWxkVGFibGUoZGcpOwogICAgfSk7CiAgICBvdXQuYXBwZW5kQ2hpbGQoYmxvY2spOwogIH0KCiAgcmVuZGVyKCk7Cn0pKCk7Cjwvc2NyaXB0Pgo8L2JvZHk+CjwvaHRtbD4K";
        public static Dictionary<string, string> ChannelNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [ChannelName.ABC.ToString()] = ChannelName.ABC.ToString(),
            [ChannelName.ACCNetwork.ToString()] = "ACCN",
            [ChannelName.BigTenNetwork.ToString()] = "BTN",
            [ChannelName.CBSSportsNetwork.ToString()] = "CBSSN",
            [ChannelName.CBS.ToString()] = ChannelName.CBS.ToString(),
            [ChannelName.CW.ToString()] = "The CW",
            [ChannelName.ESPN2.ToString()] = ChannelName.ESPN2.ToString(),
            [ChannelName.ESPN.ToString()] = ChannelName.ESPN.ToString(),
            [ChannelName.ESPNU.ToString()] = ChannelName.ESPNU.ToString(),
            [ChannelName.FOX.ToString()] = ChannelName.FOX.ToString(),
            [ChannelName.NBC.ToString()] = ChannelName.NBC.ToString(),
            [ChannelName.FoxSports1.ToString()] = "FS1",
            [ChannelName.SECNetwork.ToString()] = "SECN",
            [StreamingProvider.ESPNPlus.ToString()] = "ESPN+",
            [StreamingProvider.FoxOne.ToString()] = "FOX1",
            [StreamingProvider.ParamountPlus.ToString()] = "Paramount+",
            [StreamingProvider.Peacock.ToString()] = StreamingProvider.Peacock.ToString(),
        };

        private static SeasonCalendar currentSeason;
        public static SeasonCalendar CurrentSeason
        {
            get
            {
                if (currentSeason == null)
                {
                    currentSeason = new SeasonCalendar(Form1.DynastyYear);
                }

                return currentSeason;
            }
        }

        private static Dictionary<ChannelName, ChannelSchedule> AllNetworks = new Dictionary<ChannelName, ChannelSchedule>();
        private static Dictionary<StreamingProvider, StreamingSchedule> AllStreamers = new Dictionary<StreamingProvider, StreamingSchedule>();

        public static Dictionary<int, List<TelevisedGame>> AllGames = null;

        public static void FixTelevisionSchedule()
        {
            var team = TableUtility.FindTable("TEAM").lRecords.ToDictionary(mr => mr.TeamId());
            var games = AllGames = TableUtility.FindTable("SCHD").lRecords
                .Select(mr => new TelevisedGame(mr, team))
                .Where(g => g.GameNeedsAssignment())
                .GroupBy(g => g.ConferenceOwner)
                .ToDictionary(g => g.Key, g => g.ToList());

            // select the games
            CBSNetwork.Instance.SelectGames(games);
            CBSNetwork.Instance.AssignGames();

            NBCNetwork.Instance.SelectGames(games);
            NBCNetwork.Instance.AssignGames();

            CBSSportsNetwork.Instance.SelectGames(games);
            CBSSportsNetwork.Instance.AssignGames();

            CWNetwork.Instance.SelectGames(games);
            CWNetwork.Instance.AssignGames();

            // espn and fox now select
            ESPNNetworks.Instance.SelectGames(games);
            FoxNetworks.Instance.SelectGames(games);

            // assign the games
            ESPNNetworks.Instance.AssignGames();
            FoxNetworks.Instance.AssignGames();

            //report
            CWNetwork.Instance.Report();
            ESPNNetworks.Instance.Report();
            CBSNetwork.Instance.Report();
            NBCNetwork.Instance.Report();
            FoxNetworks.Instance.Report();
            CBSSportsNetwork.Instance.Report();

            var unassigned = games.Values.SelectMany(l => l).Where(g => g.Assigned == false).ToList();
            var json = JsonConvert.SerializeObject(
                new
                {
                    count = unassigned.Count,
                    unassigned,
                }, Formatting.Indented);
            File.WriteAllText("unassigned-games.txt", json);

            // all the games ordered by week, then day, then time
            var streaming = AllStreamers.SelectMany(s => s.Value.Games.Select(g => new { game = g.game, time = g.time, channel = s.Key.ToString() }));
            var tv = AllNetworks.SelectMany(kvp => kvp.Value.Games.Select(g => new { game = g.game, time = g.time, channel = kvp.Key.ToString() }));
            var allGames = streaming.Concat(tv)
                .OrderBy(t => t.time.Week)
                .ThenBy(t => t.time.Day)
                .ThenBy(t => t.time.GTOD)
                .Select(g => new
                {
                    Day = g.time.DayOfWeek,
                    Month = g.time.Month,
                    Date = g.time.DayOfMonth,
                    AwayRank = g.game.AwayRank,
                    HomeRank = g.game.HomeRank,
                    Home = g.game.HomeName,
                    Away = g.game.AwayName,
                    Time = g.time.ToTimeString(),
                    Channel = ChannelNames[g.channel],
                    Week = g.time.Week + 1,
                    HomeConference = g.game.HomeConference,
                    AwayConference = g.game.AwayConference,
                }).ToList();

            json = JsonConvert.SerializeObject
                (
                new
                {
                    year = CurrentSeason.Year,
                    allGames,
                });
            var html = Encoding.UTF8.GetString(Convert.FromBase64String(ScheduleHTML)).Replace(TvAllPlaceholder, json);
            File.WriteAllText("schedule.html", html);
        }

        public static bool GameNeedsAssignment(this TelevisedGame game)
        {
            var preassigner = new Func<TelevisedGame, bool>[]
                {
                    CWNetwork.Instance.PreassignGame,
                    ESPNNetworks.Instance.PreassignGame,
                    CBSNetwork.Instance.PreassignGame,
                    NBCNetwork.Instance.PreassignGame,
                    FoxNetworks.Instance.PreassignGame,
                    CBSSportsNetwork.Instance.PreassignGame,
                };

            return preassigner.All(f => f(game));
        }

        public static bool IsOctober(this int week)
        {
            return CurrentSeason.IsOctober(week);
        }

        public static bool IsAugustSeptember(this int week)
        {
            return CurrentSeason.IsAugustSeptember(week);
        }

        public static bool IsNovember(this int week)
        {
            return CurrentSeason.IsNovember(week);
        }

        public static int LaborDayWeek()
        {
            return CurrentSeason.IsLaborDayWeekendFirstWeek ? 0 : 1;
        }

        public static DateTime GetDate(this TelevisedGame game)
        {
            return CurrentSeason.GetDate(game.Week, game.Day);
        }

        public static int LastWeekOfOctober()
        {
            for (int i = CurrentSeason.Weeks.Length - 1; i >= 0; i--)
            {
                if (CurrentSeason.IsOctober(i))
                {
                    return i;
                }
            }

            throw new Exception("Bad calendar");
        }

        public static int FirstWeekOfOctober()
        {
            for (int i = 0; i <= 13; i++)
            {
                if (CurrentSeason.IsOctober(i))
                {
                    return i;
                }
            }

            throw new Exception("Bad calendar");
        }

        public static ChannelSchedule Register(this ChannelSchedule schedule)
        {
            AllNetworks[schedule.Name] = schedule;
            return schedule;
        }

        public static StreamingSchedule Register(this StreamingSchedule streamer)
        {
            AllStreamers[streamer.Provider] = streamer;
            return streamer;
        }
    }
}