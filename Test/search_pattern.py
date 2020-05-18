import cv2
import numpy as np
from matplotlib import pyplot as plt

filename = 'gmod_screenshoot_3'
img = cv2.imread(filename + '.jpg',0)
img2 = img.copy()
template = cv2.imread('gmod_pattern.png',0)
w, h = template.shape[::-1]

# All the 6 methods for comparison in a list
methods = ['cv2.TM_CCOEFF', 'cv2.TM_CCOEFF_NORMED', 'cv2.TM_CCORR',
            'cv2.TM_CCORR_NORMED', 'cv2.TM_SQDIFF', 'cv2.TM_SQDIFF_NORMED']

for meth in methods:
    img = img2.copy()
    method = eval(meth)

    # Apply template Matching
    res = cv2.matchTemplate(img,template,method)
    min_val, max_val, min_loc, max_loc = cv2.minMaxLoc(res)

    # If the method is TM_SQDIFF or TM_SQDIFF_NORMED, take minimum
    if method in [cv2.TM_SQDIFF, cv2.TM_SQDIFF_NORMED]:
        top_left = min_loc
    else:
        top_left = max_loc
    bottom_right = (top_left[0] + w, top_left[1] + h)

    cv2.rectangle(img,top_left, bottom_right, 255, 2)

    colormap = plt.get_cmap('inferno')
    heatmap = (colormap(res) * 2**16).astype(np.uint16)[:,:,:3]
    heatmap = cv2.cvtColor(heatmap, cv2.COLOR_RGB2BGR) 

    heatmap = cv2.resize(heatmap, (720, 480))
    img = cv2.resize(img, (720, 480))
    cv2.imshow('heatmap', heatmap)    
    cv2.imshow('image', img)
    cv2.waitKey()
