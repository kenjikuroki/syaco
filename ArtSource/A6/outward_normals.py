import bmesh

def fix_outward_normals(mesh):
    bm=bmesh.new(); bm.from_mesh(mesh); bm.normal_update()
    original={f:f.normal.copy() for f in bm.faces}
    bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces)); bm.normal_update()
    remaining=set(bm.faces); closed=0; open_count=0; negative_after=0
    while remaining:
        seed=remaining.pop(); island={seed}; queue=[seed]
        while queue:
            face=queue.pop()
            for edge in face.edges:
                for other in edge.link_faces:
                    if other in remaining: remaining.remove(other); island.add(other); queue.append(other)
        if not all(len(e.link_faces)==2 for f in island for e in f.edges):
            open_count+=1; continue
        closed+=1
        def volume():
            total=0.0
            for f in island:
                coords=[v.co for v in f.verts]
                for j in range(1,len(coords)-1): total+=coords[0].dot(coords[j].cross(coords[j+1]))/6
            return total
        if volume()<0: bmesh.ops.reverse_faces(bm,faces=list(island))
        if volume() < -1e-14: negative_after+=1
    bm.normal_update()
    flipped=sum(1 for f in bm.faces if original[f].dot(f.normal)<-.5)
    bm.to_mesh(mesh); bm.free(); mesh.update()
    assert negative_after==0
    return dict(mesh=mesh.name,flipped_faces=flipped,closed_islands=closed,open_islands=open_count,negative_after=negative_after)
