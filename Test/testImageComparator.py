#	 Copyright (c) 2020 Alberto Córdoba Ortiz
#
# 	  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
# 	  documentation files (the "Software"), to deal in the Software without restriction, including without limitation
# 	  the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software.

#  -----------------------------------
#  | 	PRUEBA DE CONCEPTO AVANZADA  |
#  -----------------------------------

#
#	EL SIGUIENTE CÓDIGO ES UN PROGRAMA QUE COMPARA TODAS LAS IMÁGENES DE UNA RUTA INDICADA
#	CON SUS ANÁLOGAS QUE COINCIDAN EN COMBRE DE OTRA RUTA INDICADA. UNA VEZ REALIZADA LA
#	COMPARACIÓN EL PROGRAMA GENERARÁ UNA RUTA CON LA IMAGEN RESULTADO DE LA COMPARACIÓN
#	Y UN JSON CON INFORMACIÓN QUE PUEDE SER RELEVANTE PARA ANALIZAR.
#

#IMPORTS 
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


###############################################
##                	 Rutas         			 ##
###############################################

#Path de donde busca las imágenes que se quieren comparar
SCREENSHOTS_PATH = "images/Captures/"

#Path de donde busca las imágenes originales con el contenido que se debería ver
ORIGINALS_PATH = "images/Original/"

#Path donde se almacenan las imagenes creadas con la diferencia entre las dos imagenes a comparar
RESULTS_PATH = "/result"

###############################################

###############################################################
##                	 MÉTODOS DEL PROGRAMA       			 ##
###############################################################
"""
	Dadas unas imágenes se intenta modelar el cambio 
	percibido en la información estructural de la imagen
	utilizando el algortitmo SSIM

	Parameters: 
		imageA (Array) = Primera imagen a comparar.
		imageB (Array) = Pegunda imagen a comparar.

	Return Value: Devuelve la diferencia entre imágenes tanto en porcentaje como una imagen.
"""
def SSIM(imageA, imageB) :
	(score, diff) = ssim(imageA, imageB, full=True)
	diff = (diff * 255).astype("uint8")
	#Devuelve la diferencia entre imágenes tanto en porcentaje como una imagen
	return diff, score 


"""
	Dadas las imágenes utiliza el método subtract
	de la librería CV2 para hacer diferencia de valor
	de píxeles, si la diferencia es 0 sabemos que el 
	valor es el mismo y en caso contrario el pixel es diferente

	Parameters:
		imageA (Array) = Primera imagen a comparar.
		imageB (Array) = Pegunda imagen a comparar.

	Return value: Devuelve la imagen diferencia y False en caso de no haber diferencia, True en caso contrario

"""
def comparaCV2(imageA, imageB) :
	diff = cv2.subtract(imageB, imageA)
	diff = abs(diff)

	if not np.any(diff) :
		return diff, False
	else :
		return diff, True



"""
	Carga y convierte a escala de grises una imagen.

	Parameters: 
		path (string) : Ruta de la imagen a cargar.
	
	Return Value: Devuelve la imagen original y en escala de grises.
"""
def load_and_Convert(path) : 
	image = cv2.imread(path)
	greyImage = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)
	return image, greyImage



"""
	Muestra una imagen por pantalla usando cv2.

	Parameters:
		title (string): Título de la imagen.
		image (array): Imagen a mostrar.
"""
def show_image(title, image) :
	cv2.imshow(title, image)


"""
	Crea en la carpeta donde se almacenarán las imagenes resultado  
	si esta no existe y guarda ahí las imágenes comparadas.

	Parameters: 
		imageName (string): Nombre con el que se guardará la imagen.
		im (array): Imagen a guardar.
		resultpath (string): Ruta donde se almacenará el resultado.
"""
#
def save_in_folder(imageName, im, resultpath) :
	path = os.getcwd() + resultpath
	if(not os.path.isdir(path)) :
		try:
			os.mkdir(path)
		except OSError :
			print ("Creation of the directory %s failed" % path)
			return 
		else :
			print ("%s created" % path)

	cv2.imwrite(path+ "/" + imageName + ".jpg", im)
	


"""
	Crea un id unico para cada imagen.

	Return value: Id único para identificar la imagen en el JSON.
"""
def create_id() : 
	return str(uuid.uuid4())



"""
	Guarda la información en un archivo JSON, si el archivo
	existe lo reescribe en caso contrario lo crea.

	Parameters:
		id (string): identificador único de la imagen.
		xSize (int): Escala X de la imagen.
		ySize (int): Escala Y de la imagen.
		failure (int): Valor de error (puede variar segun la estrategia utilizada).
		imageName (string): Nombre de la imagen.
"""
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
			'id' : id,
			'Image' : imageName,
			'X_Size' : xSize,
			'Y_Size' : ySize,
			'failure' : failure 
		})

	with open('Log.json', 'w') as outfile :
		json.dump(data, outfile)



"""
	Busca un archivo en una ruta

	parameters:
		name (string): Nombre del archivo.
		path (string): ruta del archivo 

	return value: Si el archivo existe lo devuelve.
"""
def find(name, path):
	for file2 in glob.glob(path + "*.png") : 
		if name == os.path.basename(file2) : 
			return file2


"""
	Comprueba si la imagen obtenida ya ha sido analizada

	Parameters:
		name (string): Nombre de la imagen.

	return Value: True en caso de ya estar analizada False en caso contrario
"""

def AlreadyAnalized(name) :
	if(os.path.exists(os.getcwd()+"/Log.json")) :
		with open('Log.json') as json_file :
			data = json.load(json_file)
			for i in data['Results'] :
				if i["Image"] == name :
					print(name + " Already Analized") 
					return True
	return False

"""
	Carga, almacena, genera JSON con información y Analiza
	las diferencias entre imagenes indicadas en las rutas.

	Parameters:
		CapturePath (string): Ruta donde podemos encontrar las capturas.
		OriginalPath (string): Ruta donde se pueden encontrar las imagenes originales para comparar.
"""
def analize(CapturePath = "", OriginalPath = "") : 
	#Analiza las imágenes de la ruta indicada como capturas con las imágenes originales
	#El path debe ser el de las imágenes capturadas

	#Comprobamos todas las imágenes de la ruta
	for file in glob.glob(CapturePath + "*.png") :
		if not AlreadyAnalized(os.path.basename(file)) : #comprobamos si la imagen ya ha sido analizada
			#Buscamos la ruta de la imagen original que queremos cargar para comparar
			org = find(os.path.basename(file), os.getcwd() +'/'+OriginalPath )
			if org != None : 
				#Cargamos y convertimos las imagenes
				original, greyOriginal = load_and_Convert(file)
				new, greyNew = load_and_Convert(org)

				#Calculamos la diferencia
				#diff, score = SSIM(greyOriginal, greyNew)

				diff, score = comparaCV2(greyOriginal, greyNew)

				#mostramos por consola la similitud SOLO PARA DEBUG
				#print("SSIM: {}".format(score))

				#Obtenemos la información para guardar la imagen diferencia y actualizar el JSON
				h, w= diff.shape
				imId = create_id()
				save_in_folder(imId, diff, RESULTS_PATH)
				export_info_toJSON(imId, w, h, score, os.path.basename(file))



#hilo inicial del programa
def main():
	analize(SCREENSHOTS_PATH, ORIGINALS_PATH)


if __name__ == "__main__":
    main()