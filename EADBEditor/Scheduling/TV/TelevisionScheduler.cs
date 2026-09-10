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
        private const string ScheduleHTML = "PCFkb2N0eXBlIGh0bWw+CjxodG1sIGxhbmc9ImVuIj4KPGhlYWQ+CjxtZXRhIGNoYXJzZXQ9InV0Zi04Ij4KPG1ldGEgbmFtZT0idmlld3BvcnQiIGNvbnRlbnQ9IndpZHRoPWRldmljZS13aWR0aCwgaW5pdGlhbC1zY2FsZT0xIj4KPHRpdGxlPkNvbGxlZ2UgRm9vdGJhbGwgVFYgU2NoZWR1bGU8L3RpdGxlPgo8c3R5bGU+CiAgOnJvb3R7CiAgICAtLWdyZWVuOiMwZDdmNDE7CiAgICAtLWdyZWVuLWRhcms6IzBhNjUzNTsKICAgIC0tYmFyLWRhcms6IzFhMWExYTsKICAgIC0tYmFyLWRhcmsyOiMyNDI0MjQ7CiAgICAtLXN0cmlwZTojZjJmMmYyOwogICAgLS1ib3JkZXI6I2UwZTBlMDsKICAgIC0tdGV4dDojMWExYTFhOwogICAgLS10ZXh0LW11dGVkOiM2NjY7CiAgICAtLWxpbms6IzBkN2Y0MTsKICAgIC0tYmc6I2ZmZmZmZjsKICAgIC0tcGFnZS1iZzojZjVmNWY1OwogICAgLS10b2RheTojZmZmOGUxOwogICAgLS10b2RheS1ib3JkZXI6I2YwYjQyOTsKICB9CiAgKntib3gtc2l6aW5nOmJvcmRlci1ib3g7fQogIGh0bWwsYm9keXttYXJnaW46MDtwYWRkaW5nOjA7fQogIGJvZHl7CiAgICBiYWNrZ3JvdW5kOnZhcigtLXBhZ2UtYmcpOwogICAgY29sb3I6dmFyKC0tdGV4dCk7CiAgICBmb250LWZhbWlseTpIZWx2ZXRpY2EsQXJpYWwsc2Fucy1zZXJpZjsKICAgIGZvbnQtc2l6ZToxNHB4OwogICAgbGluZS1oZWlnaHQ6MS40OwogIH0KICBhe2NvbG9yOnZhcigtLWxpbmspO30KICAudG9wYmFyewogICAgYmFja2dyb3VuZDp2YXIoLS1iYXItZGFyayk7CiAgICBjb2xvcjojZmZmOwogICAgcGFkZGluZzoxNHB4IDIwcHg7CiAgICBkaXNwbGF5OmZsZXg7CiAgICBhbGlnbi1pdGVtczpjZW50ZXI7CiAgICBnYXA6MTBweDsKICAgIGZsZXgtd3JhcDp3cmFwOwogIH0KICAudG9wYmFyIC5sb2dvewogICAgZm9udC13ZWlnaHQ6ODAwOwogICAgZm9udC1zaXplOjIwcHg7CiAgICBsZXR0ZXItc3BhY2luZzouM3B4OwogICAgZGlzcGxheTpmbGV4OwogICAgYWxpZ24taXRlbXM6Y2VudGVyOwogICAgZ2FwOjhweDsKICB9CiAgLnRvcGJhciAubG9nbyAuYmFsbHtmb250LXNpemU6MjBweDt9CiAgLnRvcGJhciAuc2Vhc29uewogICAgbWFyZ2luLWxlZnQ6YXV0bzsKICAgIGJhY2tncm91bmQ6dmFyKC0tZ3JlZW4pOwogICAgY29sb3I6I2ZmZjsKICAgIGZvbnQtd2VpZ2h0OjcwMDsKICAgIGZvbnQtc2l6ZToxMnB4OwogICAgcGFkZGluZzo0cHggMTBweDsKICAgIGJvcmRlci1yYWRpdXM6MTJweDsKICAgIGxldHRlci1zcGFjaW5nOi41cHg7CiAgfQogIC53cmFwewogICAgbWF4LXdpZHRoOjkwMHB4OwogICAgbWFyZ2luOjAgYXV0bzsKICAgIHBhZGRpbmc6MjBweCAxNnB4IDYwcHg7CiAgfQogIGgxewogICAgZm9udC1zaXplOjIycHg7CiAgICBtYXJnaW46NHB4IDAgOHB4OwogIH0KICAuaW50cm97CiAgICBjb2xvcjp2YXIoLS10ZXh0LW11dGVkKTsKICAgIGZvbnQtc2l6ZToxM3B4OwogICAgbWFyZ2luLWJvdHRvbToxOHB4OwogIH0KICAuY29udHJvbHN7CiAgICBiYWNrZ3JvdW5kOnZhcigtLWJnKTsKICAgIGJvcmRlcjoxcHggc29saWQgdmFyKC0tYm9yZGVyKTsKICAgIGJvcmRlci1yYWRpdXM6NnB4OwogICAgcGFkZGluZzoxNHB4OwogICAgbWFyZ2luLWJvdHRvbToxOHB4OwogIH0KICAud2Vla3Jvd3sKICAgIGRpc3BsYXk6ZmxleDsKICAgIGZsZXgtd3JhcDp3cmFwOwogICAgZ2FwOjZweDsKICAgIG1hcmdpbi1ib3R0b206MTJweDsKICB9CiAgLndlZWstYnRuewogICAgYm9yZGVyOjFweCBzb2xpZCB2YXIoLS1ib3JkZXIpOwogICAgYmFja2dyb3VuZDojZmFmYWZhOwogICAgY29sb3I6dmFyKC0tdGV4dCk7CiAgICBmb250LXNpemU6MTJweDsKICAgIGZvbnQtd2VpZ2h0OjYwMDsKICAgIHBhZGRpbmc6NnB4IDExcHg7CiAgICBib3JkZXItcmFkaXVzOjE0cHg7CiAgICBjdXJzb3I6cG9pbnRlcjsKICB9CiAgLndlZWstYnRuOmhvdmVye2JhY2tncm91bmQ6I2VlZTt9CiAgLndlZWstYnRuLmFjdGl2ZXsKICAgIGJhY2tncm91bmQ6dmFyKC0tZ3JlZW4pOwogICAgYm9yZGVyLWNvbG9yOnZhcigtLWdyZWVuKTsKICAgIGNvbG9yOiNmZmY7CiAgfQogIC5maWx0ZXJzewogICAgZGlzcGxheTpmbGV4OwogICAgZmxleC13cmFwOndyYXA7CiAgICBnYXA6MTBweDsKICAgIGFsaWduLWl0ZW1zOmNlbnRlcjsKICB9CiAgLmZpbHRlcnMgaW5wdXRbdHlwZT0ic2VhcmNoIl17CiAgICBmbGV4OjE7CiAgICBtaW4td2lkdGg6MTgwcHg7CiAgICBwYWRkaW5nOjhweCAxMHB4OwogICAgYm9yZGVyOjFweCBzb2xpZCB2YXIoLS1ib3JkZXIpOwogICAgYm9yZGVyLXJhZGl1czo0cHg7CiAgICBmb250LXNpemU6MTNweDsKICB9CiAgLmZpbHRlcnMgc2VsZWN0ewogICAgcGFkZGluZzo4cHggMTBweDsKICAgIGJvcmRlcjoxcHggc29saWQgdmFyKC0tYm9yZGVyKTsKICAgIGJvcmRlci1yYWRpdXM6NHB4OwogICAgZm9udC1zaXplOjEzcHg7CiAgICBiYWNrZ3JvdW5kOiNmZmY7CiAgfQogIC5maWx0ZXJzIGJ1dHRvbi50b2RheS1idG57CiAgICBwYWRkaW5nOjhweCAxMnB4OwogICAgYm9yZGVyOjFweCBzb2xpZCB2YXIoLS1ncmVlbik7CiAgICBiYWNrZ3JvdW5kOiNmZmY7CiAgICBjb2xvcjp2YXIoLS1ncmVlbik7CiAgICBib3JkZXItcmFkaXVzOjRweDsKICAgIGZvbnQtc2l6ZToxM3B4OwogICAgZm9udC13ZWlnaHQ6NzAwOwogICAgY3Vyc29yOnBvaW50ZXI7CiAgfQogIC5maWx0ZXJzIGJ1dHRvbi50b2RheS1idG46aG92ZXJ7YmFja2dyb3VuZDp2YXIoLS1ncmVlbik7Y29sb3I6I2ZmZjt9CgogIC5zY2hlZHVsZS10aXRsZXsKICAgIGJhY2tncm91bmQ6dmFyKC0tYmFyLWRhcmsyKTsKICAgIGNvbG9yOiNmZmY7CiAgICBmb250LXdlaWdodDo3MDA7CiAgICBmb250LXNpemU6MTRweDsKICAgIGxldHRlci1zcGFjaW5nOi40cHg7CiAgICBwYWRkaW5nOjEwcHggMTRweDsKICAgIG1hcmdpbi10b3A6MjJweDsKICAgIGJvcmRlci1yYWRpdXM6NHB4IDRweCAwIDA7CiAgfQogIC5kYXktYmxvY2t7Ym9yZGVyOjFweCBzb2xpZCB2YXIoLS1ib3JkZXIpO2JvcmRlci10b3A6bm9uZTt9CiAgLmRheS1oZWFkZXJ7CiAgICBiYWNrZ3JvdW5kOnZhcigtLWdyZWVuKTsKICAgIGNvbG9yOiNmZmY7CiAgICBmb250LXdlaWdodDo3MDA7CiAgICBmb250LXNpemU6MTNweDsKICAgIHBhZGRpbmc6OHB4IDE0cHg7CiAgfQogIC5kYXktaGVhZGVyLnRvZGF5ewogICAgYmFja2dyb3VuZDp2YXIoLS10b2RheS1ib3JkZXIpOwogICAgY29sb3I6IzNhMmEwMDsKICB9CiAgdGFibGUuZ2FtZXN7CiAgICB3aWR0aDoxMDAlOwogICAgYm9yZGVyLWNvbGxhcHNlOmNvbGxhcHNlOwogIH0KICB0YWJsZS5nYW1lcyB0aHsKICAgIGJhY2tncm91bmQ6I2U5ZTllOTsKICAgIHRleHQtYWxpZ246bGVmdDsKICAgIGZvbnQtc2l6ZToxMXB4OwogICAgdGV4dC10cmFuc2Zvcm06dXBwZXJjYXNlOwogICAgbGV0dGVyLXNwYWNpbmc6LjRweDsKICAgIGNvbG9yOnZhcigtLXRleHQtbXV0ZWQpOwogICAgcGFkZGluZzo3cHggMTRweDsKICAgIGZvbnQtd2VpZ2h0OjcwMDsKICB9CiAgdGFibGUuZ2FtZXMgdGR7CiAgICBwYWRkaW5nOjlweCAxNHB4OwogICAgYm9yZGVyLXRvcDoxcHggc29saWQgdmFyKC0tYm9yZGVyKTsKICAgIHZlcnRpY2FsLWFsaWduOm1pZGRsZTsKICAgIGZvbnQtc2l6ZToxM3B4OwogIH0KICB0YWJsZS5nYW1lcyB0ci5vZGQgdGR7YmFja2dyb3VuZDp2YXIoLS1zdHJpcGUpO30KICB0YWJsZS5nYW1lcyB0cjpob3ZlciB0ZHtiYWNrZ3JvdW5kOiNlOGYzZWM7fQogIHRhYmxlLmdhbWVzIHRyLnRvZGF5LXJvdyB0ZHtiYWNrZ3JvdW5kOnZhcigtLXRvZGF5KTt9CiAgLm1hdGNodXB7d2hpdGUtc3BhY2U6bm93cmFwO30KICAubWF0Y2h1cCAucmFua3sKICAgIGNvbG9yOnZhcigtLXRleHQtbXV0ZWQpOwogICAgZm9udC1zaXplOjExcHg7CiAgICBmb250LXdlaWdodDo3MDA7CiAgICBtYXJnaW4tcmlnaHQ6MnB4OwogIH0KICAubWF0Y2h1cCAuYXR7CiAgICBjb2xvcjp2YXIoLS10ZXh0LW11dGVkKTsKICAgIGZvbnQtc2l6ZToxMXB4OwogICAgbWFyZ2luOjAgNnB4OwogICAgdGV4dC10cmFuc2Zvcm06dXBwZXJjYXNlOwogIH0KICAubWF0Y2h1cCAudGVhbS5ob21le2ZvbnQtd2VpZ2h0OjcwMDt9CiAgLnRpbWV7d2hpdGUtc3BhY2U6bm93cmFwO2NvbG9yOnZhcigtLXRleHQtbXV0ZWQpO2ZvbnQtd2VpZ2h0OjYwMDt9CiAgLmNoYW57CiAgICBkaXNwbGF5OmlubGluZS1ibG9jazsKICAgIGJhY2tncm91bmQ6I2VlZjRmMDsKICAgIGJvcmRlcjoxcHggc29saWQgI2Q3ZTZkYzsKICAgIGNvbG9yOnZhcigtLWdyZWVuLWRhcmspOwogICAgZm9udC13ZWlnaHQ6NzAwOwogICAgZm9udC1zaXplOjExcHg7CiAgICBwYWRkaW5nOjNweCA4cHg7CiAgICBib3JkZXItcmFkaXVzOjNweDsKICAgIHdoaXRlLXNwYWNlOm5vd3JhcDsKICB9CiAgLmVtcHR5ewogICAgcGFkZGluZzozMHB4OwogICAgdGV4dC1hbGlnbjpjZW50ZXI7CiAgICBjb2xvcjp2YXIoLS10ZXh0LW11dGVkKTsKICAgIGJhY2tncm91bmQ6I2ZmZjsKICAgIGJvcmRlcjoxcHggc29saWQgdmFyKC0tYm9yZGVyKTsKICAgIGJvcmRlci10b3A6bm9uZTsKICB9CiAgLndlZWstc2VhcmNoLWxhYmVsewogICAgYmFja2dyb3VuZDojMzMzOwogICAgY29sb3I6I2ZmZjsKICAgIGZvbnQtc2l6ZToxMnB4OwogICAgZm9udC13ZWlnaHQ6NzAwOwogICAgcGFkZGluZzo4cHggMTRweDsKICAgIG1hcmdpbi10b3A6MjJweDsKICAgIGJvcmRlci1yYWRpdXM6NHB4IDRweCAwIDA7CiAgfQogIGZvb3RlcnsKICAgIHRleHQtYWxpZ246Y2VudGVyOwogICAgY29sb3I6dmFyKC0tdGV4dC1tdXRlZCk7CiAgICBmb250LXNpemU6MTFweDsKICAgIG1hcmdpbi10b3A6MzBweDsKICB9CiAgQG1lZGlhIChtYXgtd2lkdGg6NTIwcHgpewogICAgdGFibGUuZ2FtZXMgdGg6bnRoLWNoaWxkKDMpLCB0YWJsZS5nYW1lcyB0ZDpudGgtY2hpbGQoMyl7ZGlzcGxheTpub25lO30KICAgIC5tYXRjaHVwe3doaXRlLXNwYWNlOm5vcm1hbDt9CiAgfQo8L3N0eWxlPgo8L2hlYWQ+Cjxib2R5PgoKPGRpdiBjbGFzcz0idG9wYmFyIj4KICA8ZGl2IGNsYXNzPSJsb2dvIj48c3BhbiBjbGFzcz0iYmFsbCI+8J+PiDwvc3Bhbj4gQ29sbGVnZSBGb290YmFsbCBUViBTY2hlZHVsZTwvZGl2PgogIDxkaXYgY2xhc3M9InNlYXNvbiIgaWQ9InNlYXNvbkJhZGdlIj5TRUFTT048L2Rpdj4KPC9kaXY+Cgo8ZGl2IGNsYXNzPSJ3cmFwIj4KICA8aDE+Q29sbGVnZSBGb290YmFsbCBUViBTY2hlZHVsZTwvaDE+CiAgPGRpdiBjbGFzcz0iaW50cm8iPkV2ZXJ5IGdhbWUgYmVsb3csIHdpdGgga2lja29mZiB0aW1lIGFuZCBUViBjaGFubmVsLiBQaWNrIGEgd2Vlaywgc2VhcmNoIGEgdGVhbSwgb3IganVtcCB0byB0b2RheSdzIGdhbWVzLjwvZGl2PgoKICA8ZGl2IGNsYXNzPSJjb250cm9scyI+CiAgICA8ZGl2IGNsYXNzPSJ3ZWVrcm93IiBpZD0id2Vla1JvdyI+PC9kaXY+CiAgICA8ZGl2IGNsYXNzPSJmaWx0ZXJzIj4KICAgICAgPGlucHV0IHR5cGU9InNlYXJjaCIgaWQ9InRlYW1TZWFyY2giIHBsYWNlaG9sZGVyPSJTZWFyY2ggYnkgdGVhbSBuYW1l4oCmIj4KICAgICAgPHNlbGVjdCBpZD0iY2hhbm5lbEZpbHRlciI+PG9wdGlvbiB2YWx1ZT0iIj5BbGwgY2hhbm5lbHM8L29wdGlvbj48L3NlbGVjdD4KICAgICAgPHNlbGVjdCBpZD0iY29uZmVyZW5jZUZpbHRlciI+PG9wdGlvbiB2YWx1ZT0iIj5BbGwgY29uZmVyZW5jZXM8L29wdGlvbj48L3NlbGVjdD4KICAgICAgPGJ1dHRvbiBjbGFzcz0idG9kYXktYnRuIiBpZD0idG9kYXlCdG4iPkp1bXAgdG8gVG9kYXk8L2J1dHRvbj4KICAgIDwvZGl2PgogIDwvZGl2PgoKICA8ZGl2IGlkPSJzY2hlZHVsZU91dHB1dCI+PC9kaXY+CjwvZGl2PgoKPHNjcmlwdCBpZD0iZ2FtZURhdGEiIHR5cGU9ImFwcGxpY2F0aW9uL2pzb24iPnt0dmFsbH08L3NjcmlwdD4KPHNjcmlwdD4KKGZ1bmN0aW9uKCl7CiAgY29uc3QgX3NyYyA9IEpTT04ucGFyc2UoZG9jdW1lbnQuZ2V0RWxlbWVudEJ5SWQoJ2dhbWVEYXRhJykudGV4dENvbnRlbnQpOwogIGNvbnN0IGFsbEdhbWVzID0gX3NyYy5hbGxHYW1lczsKICAvLyBTb3VyY2UgZGF0YSBpcyBidWlsdCBmcm9tIGEgZml4ZWQgd2Vla2x5IHRlbXBsYXRlIChlLmcuICJXZWVrIE4gaXMgYWx3YXlzCiAgLy8gQXVndXN0IDMxIik7IHRoZSByZWFsLXdvcmxkIHdlZWtkYXkgdGhhdCBkYXRlIGZhbGxzIG9uIGRlcGVuZHMgb24gdGhlIHNlYXNvbgogIC8vIHllYXIgYW5kIGlzIGlycmVsZXZhbnQgaGVyZSwgc28gZGF5IG5hbWVzIGFyZSB0YWtlbiBmcm9tIGVhY2ggZ2FtZSdzIG93bgogIC8vICJEYXkiIGZpZWxkIHJhdGhlciB0aGFuIHJlY29tcHV0ZWQgZnJvbSB0aGUgY2FsZW5kYXIuCiAgY29uc3QgU0VBU09OX1lFQVIgPSBfc3JjLnllYXIgfHwgbmV3IERhdGUoKS5nZXRGdWxsWWVhcigpOwoKICBjb25zdCBNT05USF9JRFggPSB7SmFudWFyeTowLEZlYnJ1YXJ5OjEsTWFyY2g6MixBcHJpbDozLE1heTo0LEp1bmU6NSxKdWx5OjYsQXVndXN0OjcsU2VwdGVtYmVyOjgsT2N0b2Jlcjo5LE5vdmVtYmVyOjEwLERlY2VtYmVyOjExfTsKICBjb25zdCBNT05USF9OQU1FUyA9IFsnSmFudWFyeScsJ0ZlYnJ1YXJ5JywnTWFyY2gnLCdBcHJpbCcsJ01heScsJ0p1bmUnLCdKdWx5JywnQXVndXN0JywnU2VwdGVtYmVyJywnT2N0b2JlcicsJ05vdmVtYmVyJywnRGVjZW1iZXInXTsKICAvLyBNb250aC9EYXRlIGlzIGVhY2ggZ2FtZSdzIG93biBhY2N1cmF0ZSBkYXktb2YtbW9udGg7IERheSBpcyBqdXN0IGl0cyB3ZWVrZGF5CiAgLy8gbmFtZSBmb3IgZGlzcGxheS4gTm8gb2Zmc2V0IG1hdGggbmVlZGVkIC0tIHVzZSB0aGVtIGFzIGdpdmVuLgoKICBkb2N1bWVudC5nZXRFbGVtZW50QnlJZCgnc2Vhc29uQmFkZ2UnKS50ZXh0Q29udGVudCA9IFNFQVNPTl9ZRUFSICsgJyBTRUFTT04nOwoKICBhbGxHYW1lcy5mb3JFYWNoKGc9PnsKICAgIGcuX2RhdGUgPSBuZXcgRGF0ZShTRUFTT05fWUVBUiwgTU9OVEhfSURYW2cuTW9udGhdLCBnLkRhdGUpOwogICAgY29uc3QgbSA9IC8oXGR7MSwyfSk6KFxkezJ9KShBTXxQTSkvLmV4ZWMoZy5UaW1lKTsKICAgIGxldCBoaCA9IHBhcnNlSW50KG1bMV0sMTApLCBtbSA9IHBhcnNlSW50KG1bMl0sMTApOwogICAgaWYobVszXT09PSdQTScgJiYgaGghPT0xMikgaGgrPTEyOwogICAgaWYobVszXT09PSdBTScgJiYgaGg9PT0xMikgaGg9MDsKICAgIGcuX21pbnV0ZXMgPSBoaCo2MCttbTsKICB9KTsKCiAgY29uc3Qgd2Vla3MgPSBbLi4ubmV3IFNldChhbGxHYW1lcy5tYXAoZz0+Zy5XZWVrKSldLnNvcnQoKGEsYik9PmEtYik7CiAgY29uc3QgY2hhbm5lbHMgPSBbLi4ubmV3IFNldChhbGxHYW1lcy5tYXAoZz0+Zy5DaGFubmVsKSldLnNvcnQoKTsKICBjb25zdCBjb25mZXJlbmNlcyA9IFsuLi5uZXcgU2V0KGFsbEdhbWVzLmZsYXRNYXAoZz0+W2cuSG9tZUNvbmZlcmVuY2UsIGcuQXdheUNvbmZlcmVuY2VdKSldLmZpbHRlcihCb29sZWFuKS5zb3J0KCk7CgogIGNvbnN0IHRvZGF5ID0gbmV3IERhdGUoKTsKICB0b2RheS5zZXRIb3VycygwLDAsMCwwKTsKICBmdW5jdGlvbiBzYW1lRGF5KGEsYil7IHJldHVybiBhLmdldEZ1bGxZZWFyKCk9PT1iLmdldEZ1bGxZZWFyKCkgJiYgYS5nZXRNb250aCgpPT09Yi5nZXRNb250aCgpICYmIGEuZ2V0RGF0ZSgpPT09Yi5nZXREYXRlKCk7IH0KCiAgLy8gcGljayBkZWZhdWx0IHdlZWs6IHRoZSBvbmUgd2hvc2UgZ2FtZXMgYXJlIGNsb3Nlc3QgdG8gKGNvdmVyaW5nKSB0b2RheQogIGZ1bmN0aW9uIGRlZmF1bHRXZWVrKCl7CiAgICBsZXQgYmVzdCA9IHdlZWtzWzBdLCBiZXN0RGlmZiA9IEluZmluaXR5OwogICAgZm9yKGNvbnN0IHcgb2Ygd2Vla3MpewogICAgICBjb25zdCB3ZyA9IGFsbEdhbWVzLmZpbHRlcihnPT5nLldlZWs9PT13KTsKICAgICAgY29uc3QgbWluID0gd2cucmVkdWNlKChhLGcpPT5nLl9kYXRlPGE/Zy5fZGF0ZTphLCB3Z1swXS5fZGF0ZSk7CiAgICAgIGNvbnN0IG1heCA9IHdnLnJlZHVjZSgoYSxnKT0+Zy5fZGF0ZT5hP2cuX2RhdGU6YSwgd2dbMF0uX2RhdGUpOwogICAgICBpZih0b2RheT49bWluICYmIHRvZGF5PD1tYXgpIHJldHVybiB3OwogICAgICBjb25zdCBkaWZmID0gdG9kYXk8bWluID8gKG1pbi10b2RheSkgOiAodG9kYXktbWF4KTsKICAgICAgaWYoZGlmZiA8IGJlc3REaWZmKXsgYmVzdERpZmYgPSBkaWZmOyBiZXN0ID0gdzsgfQogICAgfQogICAgcmV0dXJuIGJlc3Q7CiAgfQoKICBsZXQgY3VycmVudFdlZWsgPSBkZWZhdWx0V2VlaygpOwogIGxldCBzZWFyY2hUZXJtID0gJyc7CiAgbGV0IGNoYW5uZWxGaWx0ZXIgPSAnJzsKICBsZXQgY29uZmVyZW5jZUZpbHRlciA9ICcnOwoKICBjb25zdCB3ZWVrUm93ID0gZG9jdW1lbnQuZ2V0RWxlbWVudEJ5SWQoJ3dlZWtSb3cnKTsKICB3ZWVrcy5mb3JFYWNoKHc9PnsKICAgIGNvbnN0IGIgPSBkb2N1bWVudC5jcmVhdGVFbGVtZW50KCdidXR0b24nKTsKICAgIGIuY2xhc3NOYW1lID0gJ3dlZWstYnRuJzsKICAgIGIudGV4dENvbnRlbnQgPSB3PT09MCA/ICdXZWVrIDAnIDogJ1dlZWsgJyt3OwogICAgYi5kYXRhc2V0LndlZWsgPSB3OwogICAgYi5hZGRFdmVudExpc3RlbmVyKCdjbGljaycsICgpPT57IGN1cnJlbnRXZWVrID0gdzsgcmVuZGVyKCk7IH0pOwogICAgd2Vla1Jvdy5hcHBlbmRDaGlsZChiKTsKICB9KTsKCiAgY29uc3QgY2hhblNlbCA9IGRvY3VtZW50LmdldEVsZW1lbnRCeUlkKCdjaGFubmVsRmlsdGVyJyk7CiAgY2hhbm5lbHMuZm9yRWFjaChjPT57CiAgICBjb25zdCBvID0gZG9jdW1lbnQuY3JlYXRlRWxlbWVudCgnb3B0aW9uJyk7CiAgICBvLnZhbHVlID0gYzsgby50ZXh0Q29udGVudCA9IGM7CiAgICBjaGFuU2VsLmFwcGVuZENoaWxkKG8pOwogIH0pOwogIGNoYW5TZWwuYWRkRXZlbnRMaXN0ZW5lcignY2hhbmdlJywgKCk9PnsgY2hhbm5lbEZpbHRlciA9IGNoYW5TZWwudmFsdWU7IHJlbmRlcigpOyB9KTsKCiAgY29uc3QgY29uZlNlbCA9IGRvY3VtZW50LmdldEVsZW1lbnRCeUlkKCdjb25mZXJlbmNlRmlsdGVyJyk7CiAgY29uZmVyZW5jZXMuZm9yRWFjaChjPT57CiAgICBjb25zdCBvID0gZG9jdW1lbnQuY3JlYXRlRWxlbWVudCgnb3B0aW9uJyk7CiAgICBvLnZhbHVlID0gYzsgby50ZXh0Q29udGVudCA9IGM7CiAgICBjb25mU2VsLmFwcGVuZENoaWxkKG8pOwogIH0pOwogIGNvbmZTZWwuYWRkRXZlbnRMaXN0ZW5lcignY2hhbmdlJywgKCk9PnsgY29uZmVyZW5jZUZpbHRlciA9IGNvbmZTZWwudmFsdWU7IHJlbmRlcigpOyB9KTsKCiAgZG9jdW1lbnQuZ2V0RWxlbWVudEJ5SWQoJ3RlYW1TZWFyY2gnKS5hZGRFdmVudExpc3RlbmVyKCdpbnB1dCcsIChlKT0+ewogICAgc2VhcmNoVGVybSA9IGUudGFyZ2V0LnZhbHVlLnRyaW0oKS50b0xvd2VyQ2FzZSgpOwogICAgcmVuZGVyKCk7CiAgfSk7CgogIGRvY3VtZW50LmdldEVsZW1lbnRCeUlkKCd0b2RheUJ0bicpLmFkZEV2ZW50TGlzdGVuZXIoJ2NsaWNrJywgKCk9PnsKICAgIGN1cnJlbnRXZWVrID0gZGVmYXVsdFdlZWsoKTsKICAgIGRvY3VtZW50LmdldEVsZW1lbnRCeUlkKCd0ZWFtU2VhcmNoJykudmFsdWUgPSAnJzsKICAgIHNlYXJjaFRlcm0gPSAnJzsKICAgIHJlbmRlcigpOwogICAgc2V0VGltZW91dCgoKT0+ewogICAgICBjb25zdCBlbCA9IGRvY3VtZW50LnF1ZXJ5U2VsZWN0b3IoJy50b2RheS1yb3csIC5kYXktaGVhZGVyLnRvZGF5Jyk7CiAgICAgIGlmKGVsKSBlbC5zY3JvbGxJbnRvVmlldyh7YmVoYXZpb3I6J3Ntb290aCcsIGJsb2NrOidjZW50ZXInfSk7CiAgICB9LCAzMCk7CiAgfSk7CgogIGZ1bmN0aW9uIGZtdERhdGUoZyl7IHJldHVybiBnLkRheSArICcsICcgKyBNT05USF9OQU1FU1tnLl9kYXRlLmdldE1vbnRoKCldICsgJyAnICsgZy5fZGF0ZS5nZXREYXRlKCk7IH0KICBmdW5jdGlvbiBmbXRUaW1lKHQpeyByZXR1cm4gdC5yZXBsYWNlKCdBTScsJyBBTScpLnJlcGxhY2UoJ1BNJywnIFBNJykudG9Mb3dlckNhc2UoKTsgfQoKICBmdW5jdGlvbiByYW5rU3BhbihyYW5rLCBuYW1lLCBpc0hvbWUpewogICAgY29uc3QgY2xzID0gJ3RlYW0nICsgKGlzSG9tZSA/ICcgaG9tZScgOiAnJyk7CiAgICBpZihyYW5rICYmIHJhbms8PTI1KXsKICAgICAgcmV0dXJuICc8c3BhbiBjbGFzcz0icmFuayI+IycrcmFuaysnPC9zcGFuPjxzcGFuIGNsYXNzPSInK2NscysnIj4nK2VzYyhuYW1lKSsnPC9zcGFuPic7CiAgICB9CiAgICByZXR1cm4gJzxzcGFuIGNsYXNzPSInK2NscysnIj4nK2VzYyhuYW1lKSsnPC9zcGFuPic7CiAgfQoKICBmdW5jdGlvbiBlc2Mocyl7CiAgICByZXR1cm4gU3RyaW5nKHMpLnJlcGxhY2UoL1smPD4iJ10vZywgYz0+KHsnJic6JyZhbXA7JywnPCc6JyZsdDsnLCc+JzonJmd0OycsJyInOicmcXVvdDsnLCInIjonJiMzOTsnfVtjXSkpOwogIH0KCiAgZnVuY3Rpb24gYnVpbGRUYWJsZShnYW1lc0luKXsKICAgIGNvbnN0IGdhbWVzID0gWy4uLmdhbWVzSW5dLnNvcnQoKGEsYik9PmEuX21pbnV0ZXMtYi5fbWludXRlcyk7CiAgICBsZXQgcm93cyA9ICcnOwogICAgZ2FtZXMuZm9yRWFjaCgoZyxpKT0+ewogICAgICBjb25zdCBpc1RvZGF5ID0gc2FtZURheShnLl9kYXRlLCB0b2RheSk7CiAgICAgIGNvbnN0IHRyQ2xzID0gKGlzVG9kYXkgPyAndG9kYXktcm93ICcgOiAnJykgKyAoaSUyPT09MSA/ICdvZGQnIDogJycpOwogICAgICByb3dzICs9ICc8dHIgY2xhc3M9IicrdHJDbHMudHJpbSgpKyciPicKICAgICAgICArICc8dGQgY2xhc3M9Im1hdGNodXAiPicrcmFua1NwYW4oZy5Bd2F5UmFuaywgZy5Bd2F5LCBmYWxzZSkrJzxzcGFuIGNsYXNzPSJhdCI+YXQ8L3NwYW4+JytyYW5rU3BhbihnLkhvbWVSYW5rLCBnLkhvbWUsIHRydWUpKyc8L3RkPicKICAgICAgICArICc8dGQgY2xhc3M9InRpbWUiPicrZm10VGltZShnLlRpbWUpKyc8L3RkPicKICAgICAgICArICc8dGQ+PHNwYW4gY2xhc3M9ImNoYW4iPicrZXNjKGcuQ2hhbm5lbCkrJzwvc3Bhbj48L3RkPicKICAgICAgICArICc8L3RyPic7CiAgICB9KTsKICAgIHJldHVybiAnPHRhYmxlIGNsYXNzPSJnYW1lcyI+PHRyPjx0aD5NYXRjaHVwPC90aD48dGg+VGltZSAoRVQpPC90aD48dGg+VFY8L3RoPjwvdHI+Jytyb3dzKyc8L3RhYmxlPic7CiAgfQoKICBmdW5jdGlvbiBncm91cEJ5RGF5KGdhbWVzKXsKICAgIGNvbnN0IG1hcCA9IG5ldyBNYXAoKTsKICAgIGdhbWVzLmZvckVhY2goZz0+ewogICAgICBjb25zdCBrZXkgPSBnLl9kYXRlLmdldFRpbWUoKTsKICAgICAgaWYoIW1hcC5oYXMoa2V5KSkgbWFwLnNldChrZXksIFtdKTsKICAgICAgbWFwLmdldChrZXkpLnB1c2goZyk7CiAgICB9KTsKICAgIHJldHVybiBbLi4ubWFwLmVudHJpZXMoKV0uc29ydCgoYSxiKT0+YVswXS1iWzBdKS5tYXAoZT0+ZVsxXSk7CiAgfQoKICBmdW5jdGlvbiBhcHBseUZpbHRlcnMoZ2FtZXMpewogICAgcmV0dXJuIGdhbWVzLmZpbHRlcihnPT57CiAgICAgIGlmKGNoYW5uZWxGaWx0ZXIgJiYgZy5DaGFubmVsIT09Y2hhbm5lbEZpbHRlcikgcmV0dXJuIGZhbHNlOwogICAgICBpZihjb25mZXJlbmNlRmlsdGVyICYmIGcuSG9tZUNvbmZlcmVuY2UhPT1jb25mZXJlbmNlRmlsdGVyICYmIGcuQXdheUNvbmZlcmVuY2UhPT1jb25mZXJlbmNlRmlsdGVyKSByZXR1cm4gZmFsc2U7CiAgICAgIGlmKHNlYXJjaFRlcm0pewogICAgICAgIGNvbnN0IGhheSA9IChnLkhvbWUrJyAnK2cuQXdheSkudG9Mb3dlckNhc2UoKTsKICAgICAgICBpZighaGF5LmluY2x1ZGVzKHNlYXJjaFRlcm0pKSByZXR1cm4gZmFsc2U7CiAgICAgIH0KICAgICAgcmV0dXJuIHRydWU7CiAgICB9KTsKICB9CgogIGZ1bmN0aW9uIHJlbmRlcigpewogICAgWy4uLndlZWtSb3cuY2hpbGRyZW5dLmZvckVhY2goYj0+Yi5jbGFzc0xpc3QudG9nZ2xlKCdhY3RpdmUnLCBOdW1iZXIoYi5kYXRhc2V0LndlZWspPT09Y3VycmVudFdlZWspKTsKCiAgICBjb25zdCBvdXQgPSBkb2N1bWVudC5nZXRFbGVtZW50QnlJZCgnc2NoZWR1bGVPdXRwdXQnKTsKICAgIG91dC5pbm5lckhUTUwgPSAnJzsKCiAgICBpZihzZWFyY2hUZXJtKXsKICAgICAgLy8gZ2xvYmFsIHNlYXJjaCBhY3Jvc3MgYWxsIHdlZWtzCiAgICAgIGNvbnN0IG1hdGNoZXMgPSBhcHBseUZpbHRlcnMoYWxsR2FtZXMpOwogICAgICBpZihtYXRjaGVzLmxlbmd0aD09PTApewogICAgICAgIG91dC5pbm5lckhUTUwgPSAnPGRpdiBjbGFzcz0id2Vlay1zZWFyY2gtbGFiZWwiPlNFQVJDSCBSRVNVTFRTPC9kaXY+PGRpdiBjbGFzcz0iZW1wdHkiPk5vIGdhbWVzIG1hdGNoIHlvdXIgc2VhcmNoLjwvZGl2Pic7CiAgICAgICAgcmV0dXJuOwogICAgICB9CiAgICAgIG91dC5pbm5lckhUTUwgKz0gJzxkaXYgY2xhc3M9IndlZWstc2VhcmNoLWxhYmVsIj5TRUFSQ0ggUkVTVUxUUyAmbWRhc2g7ICcrbWF0Y2hlcy5sZW5ndGgrJyBnYW1lJysobWF0Y2hlcy5sZW5ndGg9PT0xPycnOidzJykrJzwvZGl2Pic7CiAgICAgIGNvbnN0IGRheUdyb3VwcyA9IGdyb3VwQnlEYXkobWF0Y2hlcyk7CiAgICAgIGNvbnN0IGJsb2NrID0gZG9jdW1lbnQuY3JlYXRlRWxlbWVudCgnZGl2Jyk7CiAgICAgIGJsb2NrLmNsYXNzTmFtZSA9ICdkYXktYmxvY2snOwogICAgICBkYXlHcm91cHMuZm9yRWFjaChkZz0+ewogICAgICAgIGNvbnN0IGlzVG9kYXlHcm91cCA9IHNhbWVEYXkoZGdbMF0uX2RhdGUsIHRvZGF5KTsKICAgICAgICBibG9jay5pbm5lckhUTUwgKz0gJzxkaXYgY2xhc3M9ImRheS1oZWFkZXInKyhpc1RvZGF5R3JvdXA/JyB0b2RheSc6JycpKyciPldlZWsgJytkZ1swXS5XZWVrKycgJm1pZGRvdDsgJytmbXREYXRlKGRnWzBdKSsnPC9kaXY+JyArIGJ1aWxkVGFibGUoZGcpOwogICAgICB9KTsKICAgICAgb3V0LmFwcGVuZENoaWxkKGJsb2NrKTsKICAgICAgcmV0dXJuOwogICAgfQoKICAgIGNvbnN0IHdlZWtHYW1lcyA9IGFwcGx5RmlsdGVycyhhbGxHYW1lcy5maWx0ZXIoZz0+Zy5XZWVrPT09Y3VycmVudFdlZWspKTsKICAgIG91dC5pbm5lckhUTUwgKz0gJzxkaXYgY2xhc3M9InNjaGVkdWxlLXRpdGxlIj5DT0xMRUdFIEZPT1RCQUxMIFNDSEVEVUxFICZtZGFzaDsgV0VFSyAnK2N1cnJlbnRXZWVrKyc8L2Rpdj4nOwogICAgaWYod2Vla0dhbWVzLmxlbmd0aD09PTApewogICAgICBvdXQuaW5uZXJIVE1MICs9ICc8ZGl2IGNsYXNzPSJlbXB0eSI+Tm8gZ2FtZXMgbWF0Y2ggdGhlIHNlbGVjdGVkIGZpbHRlcnMuPC9kaXY+JzsKICAgICAgcmV0dXJuOwogICAgfQogICAgY29uc3QgZGF5R3JvdXBzID0gZ3JvdXBCeURheSh3ZWVrR2FtZXMpOwogICAgY29uc3QgYmxvY2sgPSBkb2N1bWVudC5jcmVhdGVFbGVtZW50KCdkaXYnKTsKICAgIGJsb2NrLmNsYXNzTmFtZSA9ICdkYXktYmxvY2snOwogICAgZGF5R3JvdXBzLmZvckVhY2goZGc9PnsKICAgICAgY29uc3QgaXNUb2RheUdyb3VwID0gc2FtZURheShkZ1swXS5fZGF0ZSwgdG9kYXkpOwogICAgICBibG9jay5pbm5lckhUTUwgKz0gJzxkaXYgY2xhc3M9ImRheS1oZWFkZXInKyhpc1RvZGF5R3JvdXA/JyB0b2RheSc6JycpKyciPicrZm10RGF0ZShkZ1swXSkrJzwvZGl2PicgKyBidWlsZFRhYmxlKGRnKTsKICAgIH0pOwogICAgb3V0LmFwcGVuZENoaWxkKGJsb2NrKTsKICB9CgogIHJlbmRlcigpOwp9KSgpOwo8L3NjcmlwdD4KPC9ib2R5Pgo8L2h0bWw+Cg==";
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
            [StreamingProvider.FoxOne.ToString()] = "FOX One",
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
            var fullset = TableUtility.FindTable("SCHD").lRecords
                .Select(mr => new TelevisedGame(mr, team)).ToArray();

            var gamesNeedingAssignment = fullset.Where(g => g.GameNeedsAssignment()).ToArray();
            var assignedGames = fullset.Where(g => g.Assigned).ToArray();
            var games = AllGames = gamesNeedingAssignment.GroupBy(g => g.ConferenceOwner).ToDictionary(g => g.Key, g => g.ToList());

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

            // all the games ordered by week, then day, then time
            var streaming = AllStreamers.SelectMany(s => s.Value.Games.Select(g => new { g.game, g.time, channel = s.Key.ToString() }));
            var tv = AllNetworks.SelectMany(kvp => kvp.Value.Games.Select(g => new { g.game, g.time, channel = kvp.Key.ToString() }));
            var allAssignedGames = tv.Concat(streaming).ToArray();
            var allGames = allAssignedGames
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

            // what is unassigned
            var unassigned = games.Values.SelectMany(l => l).Where(g => g.Assigned == false).ToList();
            var set = new HashSet<TelevisedGame>(allAssignedGames.Select(g => g.game));
            foreach (var game in fullset)
            {
                if (!set.Contains(game))
                {
                    unassigned.Add(game);
                }
            }

            var json = JsonConvert.SerializeObject(
                new
                {
                    count = unassigned.Count,
                    unassigned,
                }, Formatting.Indented);
            File.WriteAllText("unassigned-games.txt", json);

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