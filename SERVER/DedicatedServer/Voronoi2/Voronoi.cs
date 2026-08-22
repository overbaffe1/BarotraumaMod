using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Voronoi2
{
	// Token: 0x02000007 RID: 7
	public class Voronoi
	{
		// Token: 0x06000018 RID: 24 RVA: 0x00002420 File Offset: 0x00000620
		public Voronoi(double minDistanceBetweenSites)
		{
			this.siteidx = 0;
			this.sites = null;
			this.allEdges = null;
			this.minDistanceBetweenSites = minDistanceBetweenSites;
		}

		// Token: 0x06000019 RID: 25 RVA: 0x00002444 File Offset: 0x00000644
		public List<GraphEdge> generateVoronoi(double[] xValuesIn, double[] yValuesIn, double minX, double maxX, double minY, double maxY)
		{
			this.sort(xValuesIn, yValuesIn, xValuesIn.Length);
			if (minX > maxX)
			{
				double temp = minX;
				minX = maxX;
				maxX = temp;
			}
			if (minY > maxY)
			{
				double temp = minY;
				minY = maxY;
				maxY = temp;
			}
			this.borderMinX = minX;
			this.borderMinY = minY;
			this.borderMaxX = maxX;
			this.borderMaxY = maxY;
			this.siteidx = 0;
			this.voronoi_bd();
			return this.allEdges;
		}

		// Token: 0x0600001A RID: 26 RVA: 0x000024B8 File Offset: 0x000006B8
		private void sort(double[] xValuesIn, double[] yValuesIn, int count)
		{
			this.sites = null;
			this.allEdges = new List<GraphEdge>();
			this.nsites = count;
			this.nvertices = 0;
			this.nedges = 0;
			double sn = (double)this.nsites + 4.0;
			this.sqrt_nsites = (int)Math.Sqrt(sn);
			double[] xValues = new double[count];
			double[] yValues = new double[count];
			for (int i = 0; i < count; i++)
			{
				xValues[i] = xValuesIn[i];
				yValues[i] = yValuesIn[i];
			}
			this.sortNode(xValues, yValues, count);
		}

		// Token: 0x0600001B RID: 27 RVA: 0x0000253C File Offset: 0x0000073C
		private void qsort(Site[] sites)
		{
			List<Site> listSites = new List<Site>(sites.Length);
			for (int i = 0; i < sites.Length; i++)
			{
				listSites.Add(sites[i]);
			}
			listSites.Sort(new SiteSorterYX());
			for (int j = 0; j < sites.Length; j++)
			{
				sites[j] = listSites[j];
			}
		}

		// Token: 0x0600001C RID: 28 RVA: 0x0000258C File Offset: 0x0000078C
		private void sortNode(double[] xValues, double[] yValues, int numPoints)
		{
			this.nsites = numPoints;
			this.sites = new Site[this.nsites];
			this.xmin = xValues[0];
			this.ymin = yValues[0];
			this.xmax = xValues[0];
			this.ymax = yValues[0];
			for (int i = 0; i < this.nsites; i++)
			{
				this.sites[i] = new Site();
				this.sites[i].Coord.SetPoint(xValues[i], yValues[i]);
				this.sites[i].SiteNbr = i;
				if (xValues[i] < this.xmin)
				{
					this.xmin = xValues[i];
				}
				else if (xValues[i] > this.xmax)
				{
					this.xmax = xValues[i];
				}
				if (yValues[i] < this.ymin)
				{
					this.ymin = yValues[i];
				}
				else if (yValues[i] > this.ymax)
				{
					this.ymax = yValues[i];
				}
			}
			this.qsort(this.sites);
			this.deltax = this.xmax - this.xmin;
			this.deltay = this.ymax - this.ymin;
		}

		// Token: 0x0600001D RID: 29 RVA: 0x000026A8 File Offset: 0x000008A8
		private Site nextone()
		{
			if (this.siteidx < this.nsites)
			{
				Site s = this.sites[this.siteidx];
				this.siteidx++;
				return s;
			}
			return null;
		}

		// Token: 0x0600001E RID: 30 RVA: 0x000026E4 File Offset: 0x000008E4
		private Edge bisect(Site s1, Site s2)
		{
			Edge newedge = new Edge();
			newedge.reg[0] = s1;
			newedge.reg[1] = s2;
			newedge.ep[0] = null;
			newedge.ep[1] = null;
			double dx = s2.Coord.X - s1.Coord.X;
			double dy = s2.Coord.Y - s1.Coord.Y;
			double adx = (dx > 0.0) ? dx : (-dx);
			double ady = (dy > 0.0) ? dy : (-dy);
			newedge.c = s1.Coord.X * dx + s1.Coord.Y * dy + (dx * dx + dy * dy) * 0.5;
			if (adx > ady)
			{
				newedge.a = 1.0;
				newedge.b = dy / dx;
				newedge.c /= dx;
			}
			else
			{
				newedge.a = dx / dy;
				newedge.b = 1.0;
				newedge.c /= dy;
			}
			newedge.edgenbr = this.nedges;
			this.nedges++;
			return newedge;
		}

		// Token: 0x0600001F RID: 31 RVA: 0x0000281E File Offset: 0x00000A1E
		private void makevertex(Site v)
		{
			v.SiteNbr = this.nvertices;
			this.nvertices++;
		}

		// Token: 0x06000020 RID: 32 RVA: 0x0000283C File Offset: 0x00000A3C
		private bool PQinitialize()
		{
			this.PQcount = 0;
			this.PQmin = 0;
			this.PQhashsize = 4 * this.sqrt_nsites;
			this.PQhash = new Halfedge[this.PQhashsize];
			for (int i = 0; i < this.PQhashsize; i++)
			{
				this.PQhash[i] = new Halfedge();
			}
			return true;
		}

		// Token: 0x06000021 RID: 33 RVA: 0x00002898 File Offset: 0x00000A98
		private int PQbucket(Halfedge he)
		{
			int bucket = (int)((he.ystar - this.ymin) / this.deltay * (double)this.PQhashsize);
			if (bucket < 0)
			{
				bucket = 0;
			}
			if (bucket >= this.PQhashsize)
			{
				bucket = this.PQhashsize - 1;
			}
			if (bucket < this.PQmin)
			{
				this.PQmin = bucket;
			}
			return bucket;
		}

		// Token: 0x06000022 RID: 34 RVA: 0x000028EC File Offset: 0x00000AEC
		private void PQinsert(Halfedge he, Site v, double offset)
		{
			he.vertex = v;
			he.ystar = v.Coord.Y + offset;
			Halfedge last = this.PQhash[this.PQbucket(he)];
			Halfedge next;
			while ((next = last.PQnext) != null && (he.ystar > next.ystar || (he.ystar == next.ystar && v.Coord.X > next.vertex.Coord.X)))
			{
				last = next;
			}
			he.PQnext = last.PQnext;
			last.PQnext = he;
			this.PQcount++;
		}

		// Token: 0x06000023 RID: 35 RVA: 0x0000298C File Offset: 0x00000B8C
		private void PQdelete(Halfedge he)
		{
			if (he.vertex != null)
			{
				Halfedge last = this.PQhash[this.PQbucket(he)];
				while (last.PQnext != he)
				{
					last = last.PQnext;
				}
				last.PQnext = he.PQnext;
				this.PQcount--;
				he.vertex = null;
			}
		}

		// Token: 0x06000024 RID: 36 RVA: 0x000029E3 File Offset: 0x00000BE3
		private bool PQempty()
		{
			return this.PQcount == 0;
		}

		// Token: 0x06000025 RID: 37 RVA: 0x000029F0 File Offset: 0x00000BF0
		private DoubleVector2 PQ_min()
		{
			DoubleVector2 answer = new DoubleVector2();
			while (this.PQhash[this.PQmin].PQnext == null)
			{
				this.PQmin++;
			}
			answer.X = this.PQhash[this.PQmin].PQnext.vertex.Coord.X;
			answer.Y = this.PQhash[this.PQmin].PQnext.ystar;
			return answer;
		}

		// Token: 0x06000026 RID: 38 RVA: 0x00002A6C File Offset: 0x00000C6C
		private Halfedge PQextractmin()
		{
			Halfedge curr = this.PQhash[this.PQmin].PQnext;
			this.PQhash[this.PQmin].PQnext = curr.PQnext;
			this.PQcount--;
			return curr;
		}

		// Token: 0x06000027 RID: 39 RVA: 0x00002AB4 File Offset: 0x00000CB4
		private Halfedge HEcreate(Edge e, int pm)
		{
			return new Halfedge
			{
				ELedge = e,
				ELpm = pm,
				PQnext = null,
				vertex = null
			};
		}

		// Token: 0x06000028 RID: 40 RVA: 0x00002AE4 File Offset: 0x00000CE4
		private bool ELinitialize()
		{
			this.ELhashsize = 2 * this.sqrt_nsites;
			this.ELhash = new Halfedge[this.ELhashsize];
			for (int i = 0; i < this.ELhashsize; i++)
			{
				this.ELhash[i] = null;
			}
			this.ELleftend = this.HEcreate(null, 0);
			this.ELrightend = this.HEcreate(null, 0);
			this.ELleftend.ELleft = null;
			this.ELleftend.ELright = this.ELrightend;
			this.ELrightend.ELleft = this.ELleftend;
			this.ELrightend.ELright = null;
			this.ELhash[0] = this.ELleftend;
			this.ELhash[this.ELhashsize - 1] = this.ELrightend;
			return true;
		}

		// Token: 0x06000029 RID: 41 RVA: 0x00002BA4 File Offset: 0x00000DA4
		private Halfedge ELright(Halfedge he)
		{
			return he.ELright;
		}

		// Token: 0x0600002A RID: 42 RVA: 0x00002BAC File Offset: 0x00000DAC
		private Halfedge ELleft(Halfedge he)
		{
			return he.ELleft;
		}

		// Token: 0x0600002B RID: 43 RVA: 0x00002BB4 File Offset: 0x00000DB4
		private Site leftreg(Halfedge he)
		{
			if (he.ELedge == null)
			{
				return this.bottomsite;
			}
			if (he.ELpm != 0)
			{
				return he.ELedge.reg[1];
			}
			return he.ELedge.reg[0];
		}

		// Token: 0x0600002C RID: 44 RVA: 0x00002BE8 File Offset: 0x00000DE8
		private void ELinsert(Halfedge lb, Halfedge newHe)
		{
			newHe.ELleft = lb;
			newHe.ELright = lb.ELright;
			lb.ELright.ELleft = newHe;
			lb.ELright = newHe;
		}

		// Token: 0x0600002D RID: 45 RVA: 0x00002C10 File Offset: 0x00000E10
		private void ELdelete(Halfedge he)
		{
			he.ELleft.ELright = he.ELright;
			he.ELright.ELleft = he.ELleft;
			he.deleted = true;
		}

		// Token: 0x0600002E RID: 46 RVA: 0x00002C3C File Offset: 0x00000E3C
		private Halfedge ELgethash(int b)
		{
			if (b < 0 || b >= this.ELhashsize)
			{
				return null;
			}
			Halfedge he = this.ELhash[b];
			if (he == null || !he.deleted)
			{
				return he;
			}
			this.ELhash[b] = null;
			return null;
		}

		// Token: 0x0600002F RID: 47 RVA: 0x00002C78 File Offset: 0x00000E78
		private Halfedge ELleftbnd(DoubleVector2 p)
		{
			int bucket = (int)((p.X - this.xmin) / this.deltax * (double)this.ELhashsize);
			if (bucket < 0)
			{
				bucket = 0;
			}
			if (bucket >= this.ELhashsize)
			{
				bucket = this.ELhashsize - 1;
			}
			Halfedge he = this.ELgethash(bucket);
			if (he == null)
			{
				int i = 1;
				while (i < this.ELhashsize && (he = this.ELgethash(bucket - i)) == null && (he = this.ELgethash(bucket + i)) == null)
				{
					i++;
				}
			}
			if (he == this.ELleftend || (he != this.ELrightend && this.right_of(he, p)))
			{
				do
				{
					he = he.ELright;
				}
				while (he != this.ELrightend && this.right_of(he, p));
				he = he.ELleft;
			}
			else
			{
				do
				{
					he = he.ELleft;
				}
				while (he != this.ELleftend && !this.right_of(he, p));
			}
			if (bucket > 0 && bucket < this.ELhashsize - 1)
			{
				this.ELhash[bucket] = he;
			}
			return he;
		}

		// Token: 0x06000030 RID: 48 RVA: 0x00002D64 File Offset: 0x00000F64
		private void pushGraphEdge(Site leftSite, Site rightSite, Vector2 point1, Vector2 point2)
		{
			GraphEdge newEdge = new GraphEdge(point1, point2);
			this.allEdges.Add(newEdge);
			newEdge.Site1 = leftSite;
			newEdge.Site2 = rightSite;
		}

		// Token: 0x06000031 RID: 49 RVA: 0x00002D94 File Offset: 0x00000F94
		private void clip_line(Edge e)
		{
			double x = e.reg[0].Coord.X;
			double y = e.reg[0].Coord.Y;
			double x2 = e.reg[1].Coord.X;
			double y2 = e.reg[1].Coord.Y;
			double x3 = x2 - x;
			double y3 = y2 - y;
			if (Math.Sqrt(x3 * x3 + y3 * y3) < this.minDistanceBetweenSites)
			{
				return;
			}
			double pxmin = this.borderMinX;
			double pymin = this.borderMinY;
			double pxmax = this.borderMaxX;
			double pymax = this.borderMaxY;
			Site s;
			Site s2;
			if (e.a == 1.0 && e.b >= 0.0)
			{
				s = e.ep[1];
				s2 = e.ep[0];
			}
			else
			{
				s = e.ep[0];
				s2 = e.ep[1];
			}
			if (e.a == 1.0)
			{
				y = pymin;
				if (s != null && s.Coord.Y > pymin)
				{
					y = s.Coord.Y;
				}
				if (y > pymax)
				{
					y = pymax;
				}
				x = e.c - e.b * y;
				y2 = pymax;
				if (s2 != null && s2.Coord.Y < pymax)
				{
					y2 = s2.Coord.Y;
				}
				if (y2 < pymin)
				{
					y2 = pymin;
				}
				x2 = e.c - e.b * y2;
				if ((x > pxmax & x2 > pxmax) | (x < pxmin & x2 < pxmin))
				{
					return;
				}
				if (x > pxmax)
				{
					x = pxmax;
					y = (e.c - x) / e.b;
				}
				if (x < pxmin)
				{
					x = pxmin;
					y = (e.c - x) / e.b;
				}
				if (x2 > pxmax)
				{
					x2 = pxmax;
					y2 = (e.c - x2) / e.b;
				}
				if (x2 < pxmin)
				{
					x2 = pxmin;
					y2 = (e.c - x2) / e.b;
				}
			}
			else
			{
				x = pxmin;
				if (s != null && s.Coord.X > pxmin)
				{
					x = s.Coord.X;
				}
				if (x > pxmax)
				{
					x = pxmax;
				}
				y = e.c - e.a * x;
				x2 = pxmax;
				if (s2 != null && s2.Coord.X < pxmax)
				{
					x2 = s2.Coord.X;
				}
				if (x2 < pxmin)
				{
					x2 = pxmin;
				}
				y2 = e.c - e.a * x2;
				if ((y > pymax & y2 > pymax) | (y < pymin & y2 < pymin))
				{
					return;
				}
				if (y > pymax)
				{
					y = pymax;
					x = (e.c - y) / e.a;
				}
				if (y < pymin)
				{
					y = pymin;
					x = (e.c - y) / e.a;
				}
				if (y2 > pymax)
				{
					y2 = pymax;
					x2 = (e.c - y2) / e.a;
				}
				if (y2 < pymin)
				{
					y2 = pymin;
					x2 = (e.c - y2) / e.a;
				}
			}
			this.pushGraphEdge(e.reg[0], e.reg[1], new Vector2((float)x, (float)y), new Vector2((float)x2, (float)y2));
		}

		// Token: 0x06000032 RID: 50 RVA: 0x000030C4 File Offset: 0x000012C4
		private void endpoint(Edge e, int lr, Site s)
		{
			e.ep[lr] = s;
			if (e.ep[1 - lr] == null)
			{
				return;
			}
			this.clip_line(e);
		}

		// Token: 0x06000033 RID: 51 RVA: 0x000030E4 File Offset: 0x000012E4
		private bool right_of(Halfedge el, DoubleVector2 p)
		{
			Edge e = el.ELedge;
			Site topsite = e.reg[1];
			bool right_of_site = p.X > topsite.Coord.X;
			if (right_of_site && el.ELpm == 0)
			{
				return true;
			}
			if (!right_of_site && el.ELpm == 1)
			{
				return false;
			}
			bool above;
			if (e.a == 1.0)
			{
				double dxp = p.X - topsite.Coord.X;
				double dyp = p.Y - topsite.Coord.Y;
				bool fast = false;
				if ((!right_of_site & e.b < 0.0) | (right_of_site & e.b >= 0.0))
				{
					above = (dyp >= e.b * dxp);
					fast = above;
				}
				else
				{
					above = (p.X + p.Y * e.b > e.c);
					if (e.b < 0.0)
					{
						above = !above;
					}
					if (!above)
					{
						fast = true;
					}
				}
				if (!fast)
				{
					double dxs = topsite.Coord.X - e.reg[0].Coord.X;
					above = (e.b * (dxp * dxp - dyp * dyp) < dxs * dyp * (1.0 + 2.0 * dxp / dxs + e.b * e.b));
					if (e.b < 0.0)
					{
						above = !above;
					}
				}
			}
			else
			{
				double yl = e.c - e.a * p.X;
				double t = p.Y - yl;
				double t2 = p.X - topsite.Coord.X;
				double t3 = yl - topsite.Coord.Y;
				above = (t * t > t2 * t2 + t3 * t3);
			}
			if (el.ELpm != 0)
			{
				return !above;
			}
			return above;
		}

		// Token: 0x06000034 RID: 52 RVA: 0x000032D9 File Offset: 0x000014D9
		private Site rightreg(Halfedge he)
		{
			if (he.ELedge == null)
			{
				return this.bottomsite;
			}
			if (he.ELpm != 0)
			{
				return he.ELedge.reg[0];
			}
			return he.ELedge.reg[1];
		}

		// Token: 0x06000035 RID: 53 RVA: 0x00003310 File Offset: 0x00001510
		private double dist(Site s, Site t)
		{
			double dx = s.Coord.X - t.Coord.X;
			double dy = s.Coord.Y - t.Coord.Y;
			return Math.Sqrt(dx * dx + dy * dy);
		}

		// Token: 0x06000036 RID: 54 RVA: 0x0000335C File Offset: 0x0000155C
		private Site intersect(Halfedge el1, Halfedge el2)
		{
			Edge e = el1.ELedge;
			Edge e2 = el2.ELedge;
			if (e == null || e2 == null)
			{
				return null;
			}
			if (e.reg[1] == e2.reg[1])
			{
				return null;
			}
			double d = e.a * e2.b - e.b * e2.a;
			if (-1E-10 < d && d < 1E-10)
			{
				return null;
			}
			double xint = (e.c * e2.b - e2.c * e.b) / d;
			double yint = (e2.c * e.a - e.c * e2.a) / d;
			Halfedge el3;
			Edge e3;
			if (e.reg[1].Coord.Y < e2.reg[1].Coord.Y || (e.reg[1].Coord.Y == e2.reg[1].Coord.Y && e.reg[1].Coord.X < e2.reg[1].Coord.X))
			{
				el3 = el1;
				e3 = e;
			}
			else
			{
				el3 = el2;
				e3 = e2;
			}
			bool right_of_site = xint >= e3.reg[1].Coord.X;
			if ((right_of_site && el3.ELpm == 0) || (!right_of_site && el3.ELpm == 1))
			{
				return null;
			}
			return new Site
			{
				Coord = 
				{
					X = xint,
					Y = yint
				}
			};
		}

		// Token: 0x06000037 RID: 55 RVA: 0x000034E4 File Offset: 0x000016E4
		private bool voronoi_bd()
		{
			DoubleVector2 newintstar = null;
			this.PQinitialize();
			this.ELinitialize();
			this.bottomsite = this.nextone();
			Site newsite = this.nextone();
			for (;;)
			{
				if (!this.PQempty())
				{
					newintstar = this.PQ_min();
				}
				if (newsite != null && (this.PQempty() || newsite.Coord.Y < newintstar.Y || (newsite.Coord.Y == newintstar.Y && newsite.Coord.X < newintstar.X)))
				{
					Halfedge lbnd = this.ELleftbnd(newsite.Coord);
					Halfedge rbnd = this.ELright(lbnd);
					Site bot = this.rightreg(lbnd);
					Edge e = this.bisect(bot, newsite);
					Halfedge bisector = this.HEcreate(e, 0);
					this.ELinsert(lbnd, bisector);
					Site p;
					if ((p = this.intersect(lbnd, bisector)) != null)
					{
						this.PQdelete(lbnd);
						this.PQinsert(lbnd, p, this.dist(p, newsite));
					}
					lbnd = bisector;
					bisector = this.HEcreate(e, 1);
					this.ELinsert(lbnd, bisector);
					if ((p = this.intersect(bisector, rbnd)) != null)
					{
						this.PQinsert(bisector, p, this.dist(p, newsite));
					}
					newsite = this.nextone();
				}
				else
				{
					if (this.PQempty())
					{
						break;
					}
					Halfedge lbnd = this.PQextractmin();
					Halfedge llbnd = this.ELleft(lbnd);
					Halfedge rbnd = this.ELright(lbnd);
					Halfedge rrbnd = this.ELright(rbnd);
					Site bot = this.leftreg(lbnd);
					Site top = this.rightreg(rbnd);
					Site v = lbnd.vertex;
					this.makevertex(v);
					this.endpoint(lbnd.ELedge, lbnd.ELpm, v);
					this.endpoint(rbnd.ELedge, rbnd.ELpm, v);
					this.ELdelete(lbnd);
					this.PQdelete(rbnd);
					this.ELdelete(rbnd);
					int pm = 0;
					if (bot.Coord.Y > top.Coord.Y)
					{
						Site temp = bot;
						bot = top;
						top = temp;
						pm = 1;
					}
					Edge e = this.bisect(bot, top);
					Halfedge bisector = this.HEcreate(e, pm);
					this.ELinsert(llbnd, bisector);
					this.endpoint(e, 1 - pm, v);
					Site p;
					if ((p = this.intersect(llbnd, bisector)) != null)
					{
						this.PQdelete(llbnd);
						this.PQinsert(llbnd, p, this.dist(p, bot));
					}
					if ((p = this.intersect(bisector, rrbnd)) != null)
					{
						this.PQinsert(bisector, p, this.dist(p, bot));
					}
				}
			}
			for (Halfedge lbnd = this.ELright(this.ELleftend); lbnd != this.ELrightend; lbnd = this.ELright(lbnd))
			{
				Edge e = lbnd.ELedge;
				this.clip_line(e);
			}
			return true;
		}

		// Token: 0x06000038 RID: 56 RVA: 0x0000379C File Offset: 0x0000199C
		public List<GraphEdge> MakeVoronoiGraph(List<Vector2> sites, int width, int height)
		{
			double[] xVal = new double[sites.Count];
			double[] yVal = new double[sites.Count];
			for (int i = 0; i < sites.Count; i++)
			{
				xVal[i] = (double)sites[i].X;
				yVal[i] = (double)sites[i].Y;
			}
			return this.generateVoronoi(xVal, yVal, 0.0, (double)width, 0.0, (double)height);
		}

		// Token: 0x06000039 RID: 57 RVA: 0x00003810 File Offset: 0x00001A10
		public List<GraphEdge> MakeVoronoiGraph(double[] xVal, double[] yVal, int width, int height)
		{
			return this.generateVoronoi(xVal, yVal, 0.0, (double)width, 0.0, (double)height);
		}

		// Token: 0x0600003A RID: 58 RVA: 0x00003834 File Offset: 0x00001A34
		public List<GraphEdge> MakeVoronoiGraph(double[] xVal, double[] yVal, Rectangle area)
		{
			for (int i = 0; i < xVal.Length; i++)
			{
				xVal[i] -= (double)area.X;
				yVal[i] -= (double)area.Y;
			}
			List<GraphEdge> graphEdges = this.generateVoronoi(xVal, yVal, 0.0, (double)area.Width, 0.0, (double)area.Height);
			HashSet<Site> sites = new HashSet<Site>();
			foreach (GraphEdge graphEdge in graphEdges)
			{
				graphEdge.Point1 += area.Location.ToVector2();
				graphEdge.Point2 += area.Location.ToVector2();
				sites.Add(graphEdge.Site1);
				sites.Add(graphEdge.Site2);
			}
			foreach (Site site in sites)
			{
				site.Coord = new DoubleVector2(site.Coord.X + (double)area.Location.X, site.Coord.Y + (double)area.Location.Y);
			}
			return graphEdges;
		}

		// Token: 0x0400000E RID: 14
		private double borderMinX;

		// Token: 0x0400000F RID: 15
		private double borderMaxX;

		// Token: 0x04000010 RID: 16
		private double borderMinY;

		// Token: 0x04000011 RID: 17
		private double borderMaxY;

		// Token: 0x04000012 RID: 18
		private int siteidx;

		// Token: 0x04000013 RID: 19
		private double xmin;

		// Token: 0x04000014 RID: 20
		private double xmax;

		// Token: 0x04000015 RID: 21
		private double ymin;

		// Token: 0x04000016 RID: 22
		private double ymax;

		// Token: 0x04000017 RID: 23
		private double deltax;

		// Token: 0x04000018 RID: 24
		private double deltay;

		// Token: 0x04000019 RID: 25
		private int nvertices;

		// Token: 0x0400001A RID: 26
		private int nedges;

		// Token: 0x0400001B RID: 27
		private int nsites;

		// Token: 0x0400001C RID: 28
		private Site[] sites;

		// Token: 0x0400001D RID: 29
		private Site bottomsite;

		// Token: 0x0400001E RID: 30
		private int sqrt_nsites;

		// Token: 0x0400001F RID: 31
		private double minDistanceBetweenSites;

		// Token: 0x04000020 RID: 32
		private int PQcount;

		// Token: 0x04000021 RID: 33
		private int PQmin;

		// Token: 0x04000022 RID: 34
		private int PQhashsize;

		// Token: 0x04000023 RID: 35
		private Halfedge[] PQhash;

		// Token: 0x04000024 RID: 36
		private const int LE = 0;

		// Token: 0x04000025 RID: 37
		private const int RE = 1;

		// Token: 0x04000026 RID: 38
		private int ELhashsize;

		// Token: 0x04000027 RID: 39
		private Halfedge[] ELhash;

		// Token: 0x04000028 RID: 40
		private Halfedge ELleftend;

		// Token: 0x04000029 RID: 41
		private Halfedge ELrightend;

		// Token: 0x0400002A RID: 42
		private List<GraphEdge> allEdges;
	}
}
