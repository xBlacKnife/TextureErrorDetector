#	 Copyright (c) 2020 Alberto Córdoba Ortiz
#
# 	  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
# 	  documentation files (the "Software"), to deal in the Software without restriction, including without limitation
# 	  the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, 
# 	  and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

#  -----------------------------------
#  | 	PRUEBA DE CONCEPTO AVANZADA  |
#  -----------------------------------
import PIL
from PIL import Image
from skimage.metrics import structural_similarity as ssim
import matplotlib.pyplot as plt
import numpy as np
import argparse
import imutils
import uuid
import glob
import os
import json
import cv2


#Devuelve la diferencia entre imágenes tanto en porcentaje como una imagen
def SSIM(imageA, imageB) :
	(score, diff) = ssim(imageA, imageB, full=True)
	diff = (diff * 255).astype("uint8")
	return diff, score

#Carga y convierte a escala de grises una imagen. La devuelve original y en escala de grises
def load_and_Convert(path) : 
	image = cv2.imread(path)
	greyImage = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)
	return image, greyImage

#Muestra una imagen por pantalla
def show_image(title, image) :
	cv2.imshow(title, image)

#Crea el la carpeta result si esta no existe y guarda ahí las imágenes comparadas
def save_in_folder(imageName, im) :
	path = os.getcwd() + "/result"
	if(not os.path.isdir(path)) :
		try:
			os.mkdir(path)
		except OSError :
			print ("Creation of the directory %s failed" % path)
			return 
		else :
			print ("%s created" % path)

	cv2.imwrite(path+ "/" + imageName + ".jpg", im)
	
#Crea un id unico para cada imagen
def create_id() : 
	return str(uuid.uuid4())

#Guarda la información en un archivo JSON
def export_info_toJSON(id, xSize, ySize, failure, imageName) :
	if(not os.path.exists(os.getcwd()+"/Log.json")) :
		data = {}
		data['Results'] = []
		with open('Log.json', 'w') as outfile :
			json.dump(data, outfile)
			outfile.close()

	with open('Log.json') as json_file :
		data = json.load(json_file)
		data['Results'].append({
			'name' : id,
			'Image' : imageName,
			'X_Size' : xSize,
			'Y_Size' : ySize,
			'failure' : failure 
		})

	with open('Log.json', 'w') as outfile :
		json.dump(data, outfile)

#Busca un archivo en una ruta
def find(name, path):
	for file2 in glob.glob(path + "*.png") : 
		if name == os.path.basename(file2) : 
			return file2

#Comprueba si la imagen obtenida ya ha sido analizada
def AlreadyAnalized(name) :
	if(os.path.exists(os.getcwd()+"/Log.json")) :
		with open('Log.json') as json_file :
			data = json.load(json_file)
			print(data['Results'])
			for i in data['Results'] :
				print(i)
				if i["Image"] == name :
					print(name)
					return True
	return False

#Analiza las imágenes de la ruta indicada como capturas con las imágenes originales
#El path debe ser el de las imágenes capturadas
def analize(CapturePath = "", OriginalPath = "") : 
	for file in glob.glob(CapturePath + "*.png") :
		if not AlreadyAnalized(os.path.basename(file)) :
			org = find(os.path.basename(file), os.getcwd() +'/'+OriginalPath )
			original, greyOriginal = load_and_Convert(file)
			new, greyNew = load_and_Convert(org)
			diff, score = SSIM(greyOriginal, greyNew) 
			print("SSIM: {}".format(score))
			h, w= diff.shape
			imId = create_id()
			save_in_folder(imId, diff)
			export_info_toJSON(imId, w, h, score, os.path.basename(file))

analize("images/Captures/", "images/Original/")


