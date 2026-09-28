"""Original audition-only compositions. Does not edit Unity assets or scenes."""
import os, wave, json
import numpy as np

OUT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', 'QA', 'BattleMusicAuditionsV2'))
os.makedirs(OUT, exist_ok=True)
SR = 44100
TAU = 2 * np.pi

def hz(note): return 440 * 2 ** ((note - 69) / 12)

def compose(kind, bpm, name):
    rng = np.random.default_rng(900 + kind)
    size = round(SR * 60 / bpm * 32)
    beat = size / SR / 32
    mix = np.zeros((size, 2), np.float64)

    def put(at, duration, sound, gain=1, pan=0):
        t = np.arange(round(duration * SR)) / SR
        sig = sound(t)
        sig = sig * np.minimum(1, t / .002) * np.clip((duration - t) / .025, 0, 1) * gain
        index = (round(at * beat * SR) + np.arange(len(t))) % size
        mix[index, 0] += sig * np.sqrt((1 - pan) / 2)
        mix[index, 1] += sig * np.sqrt((1 + pan) / 2)

    def noise(t, cutoff=2400):
        x = rng.normal(0, 1, len(t))
        f = np.fft.rfftfreq(len(t), 1 / SR)
        return np.fft.irfft(np.fft.rfft(x) / (1 + (f / cutoff) ** 4), n=len(t))

    def kick(t):
        return np.sin(TAU * (56*t + 2.7*(1-np.exp(-t*42))))*np.exp(-t*17) + .12*noise(t, 3500)*np.exp(-t*110)
    def snare(t):
        return .72*noise(t, 2900)*np.exp(-t*26) + .36*np.sin(TAU*185*t)*np.exp(-t*35)
    def tom(note):
        return lambda t: (np.sin(TAU*(hz(note)*t + .9*(1-np.exp(-t*35)))) + .45*noise(t, 1600))*np.exp(-t*19)
    def hat(t): return noise(t, 2600)*np.exp(-t*65)*.18
    def bass(note):
        f = hz(note)
        return lambda t: np.tanh(1.3*(np.sin(TAU*f*t)+.30*np.sin(TAU*2*f*t)+.12*np.sin(TAU*3*f*t)))*np.exp(-t*7)*(1-np.exp(-t*240))
    def drive(note):
        # Low bowed/saturated pulse: no FM bell or ringing upper-register notes.
        f = hz(note)
        return lambda t: np.tanh(1.5*(np.sin(TAU*f*t)+.38*np.sin(TAU*2*f*t)+.18*np.sin(TAU*3*f*t)+.08*np.sin(TAU*5*f*t)))*np.exp(-t*11)*(1-np.exp(-t*100))
    roots = [38,38,34,34,31,31,33,33]
    for bar in range(8):
        start = bar*4
        root = roots[bar]
        # Short, dark harmonic swells tie all three treatments to the same underwater world.
        for interval, pan in [(12,-.55),(19,.55),(22,-.2)]:
            f = hz(root+interval)
            put(start, beat*4+.25, lambda t, f=f: np.sin(np.pi*t/(beat*4+.25))**2 * (np.sin(TAU*f*t+.15*np.sin(t*3))+.15*np.sin(TAU*f*2*t)), .022, pan)
        if kind == 0:
            # Heavy, syncopated tribal percussion: deliberate advance / strike / retreat.
            for step in [0,1.5,2.75]: put(start+step,.38,kick,.63 if step==0 else .46)
            for step in [1,3]: put(start+step,.24,snare,.34, .1)
            for step,note in [(0.75,40),(2.25,36),(3.5,43)]: put(start+step,.42,tom(note),.40,(-1 if step<2 else 1)*.35)
            for step in [0,.75,1.5,2.75,3.5]: put(start+step,.32,bass(root),.30)
            for step in np.arange(.5,4,.5): put(start+float(step),.10,hat,.21, .3)
            for step in [.25,1,1.75,2.5,3.25]: put(start+step,.24,drive(root+12),.22,-.15)
        elif kind == 1:
            # Fast breakbeat and clipped bass: the most arcade/action-oriented option.
            for step in [0,1.5,2,2.75]: put(start+step,.30,kick,.53)
            for step in [1,3]: put(start+step,.23,snare,.49)
            for step in np.arange(0,4,.5): put(start+float(step),.095,hat,.42 if step%1 else .24,(-1 if step%1 else 1)*.4)
            for step in [0,.5,.75,1.5,2.25,2.75,3.5]: put(start+step,.23,bass(root+(12 if step==3.5 else 0)),.28)
            motif = [root+12,root+12,root+19,root+10]
            for j,step in enumerate([.25,.75,1.75,2.5,3.25,3.75]): put(start+step,.19,drive(motif[(j+bar%2)%4]),.25,(-1 if j%2 else 1)*.20)
        else:
            # Cinematic duel: half-time drums, threatening ostinato and rising fills.
            for step in [0,.75,2.5]: put(start+step,.48,kick,.67 if step==0 else .43)
            put(start+2,.32,snare,.48)
            for j,step in enumerate([.5,1.5,2.75,3.25,3.75]): put(start+step,.38,tom(43-j%3*3),.25,(-1 if j%2 else 1)*.32)
            for step in [0,.75,1.5,2.5,3.25]: put(start+step,.42,bass(root),.34)
            for j,step in enumerate(np.arange(0,4,.5)):
                note = root + [12,12,19,12,15,12,17,11][j]
                put(start+float(step),.27,drive(note),.23 if j%2 else .30,(-1 if j%2 else 1)*.15)
            if bar%2==0:
                put(start, .8, lambda t: noise(t, 1100)*np.exp(-t*5) + .35*np.sin(TAU*95*t)*np.exp(-t*6), .35)
        if bar in [3,7]:
            for j in range(4): put(start+3+j*.25,.25,tom(43-j*3),.30, .3-j*.2)
        # A filtered surge at phrase changes; kept behind the percussion.
        if bar%2==0: put(start,1.4,lambda t: noise(t,800)*np.sin(np.pi*t/1.4)**2,.09,-.4)

    # Short circular stereo reflections, not a long reverb that blurs transients.
    dry = mix.copy()
    mix += np.roll(dry[:,::-1],round(beat*.75*SR),axis=0)*.08
    mix += np.roll(dry,round(beat*1.5*SR),axis=0)*.04
    mix -= mix.mean(axis=0)
    mix = np.tanh(mix*1.25)
    peak = np.max(np.abs(mix))
    # Match the auditions' average level so selection is not just "louder wins".
    rms = np.sqrt(np.mean(mix**2))
    mix *= min(.16/rms, .86/peak)
    seam = float(np.max(np.abs(mix[0]-mix[-1])))
    assert np.isfinite(mix).all() and np.max(np.abs(mix))<=.861
    assert seam<.025, (name,seam)
    pcm = (mix*32767).astype('<i2')
    path = os.path.join(OUT,name+'.wav')
    with wave.open(path,'wb') as f:
        f.setnchannels(2); f.setsampwidth(2); f.setframerate(SR); f.writeframes(pcm.tobytes())
    return dict(file=path,bpm=bpm,seconds=size/SR,peak=float(np.max(np.abs(mix))),rms=float(np.sqrt(np.mean(mix**2))),seam=seam)

results=[compose(0,120,'A_HeavyCombat'),compose(1,132,'B_RushCombat'),compose(2,112,'C_DeepDuel')]
with open(os.path.join(OUT,'verification.json'),'w') as f: json.dump(results,f,indent=2)
print(json.dumps(results,indent=2))
