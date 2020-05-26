from skimage.metrics import structural_similarity as ssim
import matplotlib.pyplot as plt
import numpy as np
import argparse
import imutils
import cv2


#  -----------------------------------
#  | 	PRIMERA PRUEBA DE CONCEPTO 	 |
#  -----------------------------------

# load the two input images
original = cv2.imread("images/gmod_screenshoot_1.jpg")
new = cv2.imread("images/gmod_screenshoot_2.jpg")
# convert the images to grayscale
grayA = cv2.cvtColor(original, cv2.COLOR_BGR2GRAY)
grayB = cv2.cvtColor(new, cv2.COLOR_BGR2GRAY)

(score, diff) = ssim(grayA, grayB, full=True)
diff = (diff * 255).astype("uint8")
print("SSIM: {}".format(score))

thresh = cv2.threshold(diff, 0, 255,
	cv2.THRESH_BINARY_INV | cv2.THRESH_OTSU)[1]
cnts = cv2.findContours(thresh.copy(), cv2.RETR_EXTERNAL,
	cv2.CHAIN_APPROX_SIMPLE)
cnts = imutils.grab_contours(cnts)

cv2.imshow("Original", original)
cv2.imshow("Modified", new)
cv2.imshow("Diff", diff)
cv2.waitKey(0)